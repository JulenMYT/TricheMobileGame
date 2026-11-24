using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RoomService : MonoBehaviour
{
    public const int MaxPlayers = 10;

    // Lobby considère un salon inactif après 30 s sans heartbeat de l'hôte :
    // il n'est alors plus possible de le rejoindre.
    private const int HeartbeatIntervalMs = 15000;

    // GetLobby est limité à 1 requête par seconde et par joueur : on reste en dessous.
    private const int RefreshIntervalMs = 1500;

    // WebSockets sécurisés partout : obligatoire pour WebGL, et Relay exige que
    // l'hôte et les clients utilisent le même type de connexion.
    private const string RelayConnectionType = "wss";

    private const string PlayerNameKey = "PlayerName";
    private const string RelayCodeKey = "RelayCode";

    [SerializeField] private NetworkManager networkManager;

    private Lobby currentLobby;
    private bool isHost;
    private bool leaving;

    // Incrémenté à chaque salon : les boucles d'un ancien salon s'arrêtent d'elles-mêmes.
    private int sessionId;

    // Appelé quand la connexion au salon est perdue sans qu'on l'ait quitté
    // (hôte parti, salon supprimé, réseau coupé). Le texte est à afficher au joueur.
    public event Action<string> Disconnected;

    public Lobby CurrentLobby => currentLobby;
    public bool IsHost => isHost;
    public int PlayerCount => currentLobby != null ? currentLobby.Players.Count : 0;
    public string RoomCode => currentLobby?.LobbyCode;

    private void Awake()
    {
        ServiceLocator.Register<RoomService>(this);
    }

    private void OnEnable()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDisable()
    {
        if (networkManager != null)
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void OnApplicationQuit()
    {
        // Libère le salon tout de suite au lieu d'attendre son expiration.
        if (currentLobby != null)
            _ = LeaveRoom();
    }

    public async Task CreateRoom(string playerName)
    {
        await ServiceLocator.Get<NetworkServices>().Initialize();

        Allocation allocation =
            await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);

        string relayJoinCode =
            await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            // Privé : on ne le rejoint qu'avec son code, il n'apparaît dans aucune liste.
            IsPrivate = true,
            Player = CreatePlayer(playerName),
            Data = new Dictionary<string, DataObject>
            {
                {
                    RelayCodeKey,
                    new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)
                }
            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
            "Imposteur",
            MaxPlayers,
            options
        );

        // Le salon doit être connu AVANT StartHost : OnlineGame s'enregistre
        // pendant StartHost et lit le nom du joueur dans le salon courant.
        BeginSession(lobby, true);

        try
        {
            ConfigureTransport(new RelayServerData(allocation, RelayConnectionType));

            if (!networkManager.StartHost())
                throw new Exception("Impossible de démarrer l'hôte.");
        }
        catch
        {
            EndSession();
            await TryDeleteLobby(lobby.Id);
            throw;
        }
    }

    public async Task JoinRoom(string playerName, string roomCode)
    {
        await ServiceLocator.Get<NetworkServices>().Initialize();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
            roomCode,
            new JoinLobbyByCodeOptions { Player = CreatePlayer(playerName) }
        );

        try
        {
            string relayJoinCode = lobby.Data[RelayCodeKey].Value;

            JoinAllocation allocation =
                await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            ConfigureTransport(new RelayServerData(allocation, RelayConnectionType));

            // Comme pour l'hôte : le salon doit être connu avant la connexion.
            BeginSession(lobby, false);

            if (!networkManager.StartClient())
                throw new Exception("Impossible de démarrer le client.");
        }
        catch
        {
            // On ne reste pas inscrit dans un salon qu'on n'a pas pu rejoindre.
            EndSession();
            await TryRemoveLocalPlayer(lobby.Id);
            throw;
        }
    }

    public async Task LeaveRoom()
    {
        if (currentLobby == null)
            return;

        leaving = true;

        string lobbyId = currentLobby.Id;
        bool wasHost = isHost;

        EndSession();

        if (networkManager.IsListening)
            networkManager.Shutdown();

        if (wasHost)
            await TryDeleteLobby(lobbyId);
        else
            await TryRemoveLocalPlayer(lobbyId);

        leaving = false;
    }

    public string GetLocalPlayerName()
    {
        if (currentLobby == null)
            return null;

        string playerId = AuthenticationService.Instance.PlayerId;
        Player player = currentLobby.Players.Find(p => p.Id == playerId);

        if (player == null || !player.Data.TryGetValue(PlayerNameKey, out PlayerDataObject name))
            return null;

        return name.Value;
    }

    private static Player CreatePlayer(string playerName)
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    PlayerNameKey,
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                }
            }
        };
    }

    private void ConfigureTransport(RelayServerData relayServerData)
    {
        UnityTransport transport = networkManager.GetComponent<UnityTransport>();

        transport.UseWebSockets = true;
        transport.SetRelayServerData(relayServerData);
    }

    private void BeginSession(Lobby lobby, bool host)
    {
        currentLobby = lobby;
        isHost = host;
        sessionId++;

        int session = sessionId;

        _ = RefreshLobbyRoutine(session);

        if (host)
            _ = HeartbeatRoutine(session);
    }

    private void EndSession()
    {
        currentLobby = null;
        isHost = false;
        sessionId++;
    }

    private bool IsSessionActive(int session) =>
        session == sessionId && currentLobby != null;

    private async Task HeartbeatRoutine(int session)
    {
        while (IsSessionActive(session))
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Heartbeat du salon échoué : {exception.Message}");
            }

            await Task.Delay(HeartbeatIntervalMs);
        }
    }

    private async Task RefreshLobbyRoutine(int session)
    {
        while (IsSessionActive(session))
        {
            try
            {
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

                if (!IsSessionActive(session))
                    return;

                currentLobby = lobby;
                ServiceLocator.Get<RoomPanel>().SetPlayers(lobby.Players);
            }
            catch (LobbyServiceException exception) when (
                exception.Reason == LobbyExceptionReason.LobbyNotFound ||
                exception.Reason == LobbyExceptionReason.Forbidden)
            {
                // Salon supprimé par l'hôte, ou on en a été retiré.
                if (IsSessionActive(session))
                    LoseConnection("Le salon a été fermé.");

                return;
            }
            catch (Exception exception)
            {
                // Erreur passagère (limite de requêtes, réseau) : on réessaie plus tard
                // au lieu d'arrêter définitivement la mise à jour de la liste.
                Debug.LogWarning($"Mise à jour du salon échouée : {exception.Message}");
            }

            await Task.Delay(RefreshIntervalMs);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // Côté client, ce callback signale la perte de la connexion avec l'hôte.
        if (leaving || currentLobby == null || isHost)
            return;

        if (clientId != networkManager.LocalClientId && clientId != NetworkManager.ServerClientId)
            return;

        LoseConnection("La connexion avec l'hôte a été perdue.");
    }

    private void LoseConnection(string message)
    {
        string lobbyId = currentLobby?.Id;

        EndSession();

        if (networkManager.IsListening)
            networkManager.Shutdown();

        if (lobbyId != null)
            _ = TryRemoveLocalPlayer(lobbyId);

        Disconnected?.Invoke(message);
    }

    private static async Task TryDeleteLobby(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Suppression du salon échouée : {exception.Message}");
        }
    }

    private static async Task TryRemoveLocalPlayer(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(
                lobbyId,
                AuthenticationService.Instance.PlayerId
            );
        }
        catch (Exception exception)
        {
            // Déjà retiré ou salon déjà supprimé : rien à faire.
            Debug.LogWarning($"Sortie du salon échouée : {exception.Message}");
        }
    }

    // Transforme une erreur technique en message compréhensible pour le joueur.
    public static string GetErrorMessage(Exception exception)
    {
        if (exception is LobbyServiceException lobbyException)
        {
            switch (lobbyException.Reason)
            {
                case LobbyExceptionReason.LobbyNotFound:
                case LobbyExceptionReason.InvalidJoinCode:
                    return "Aucun salon avec ce code.";
                case LobbyExceptionReason.LobbyFull:
                    return "Ce salon est complet.";
                case LobbyExceptionReason.RateLimited:
                    return "Trop de tentatives, réessaie dans quelques secondes.";
                case LobbyExceptionReason.NetworkError:
                    return "Pas de connexion internet.";
            }
        }

        if (exception is RequestFailedException)
            return "Connexion aux serveurs impossible.";

        return "Une erreur est survenue, réessaie.";
    }
}

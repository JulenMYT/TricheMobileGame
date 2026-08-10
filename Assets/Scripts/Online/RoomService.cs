using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RoomService : MonoBehaviour
{
    private const int MaxPlayers = 10;

    private Lobby currentLobby;

    [SerializeField] private NetworkManager networkManager;

    private void Awake()
    {
        ServiceLocator.Register<RoomService>(this);
    }

    public async Task<Lobby> CreateRoom(string playerName)
    {
        await ServiceLocator.Get<NetworkServices>().Initialize();

        string roomCode = GenerateRoomCode();

        Allocation allocation =
            await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);

        string relayJoinCode =
            await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        UnityTransport transport =
            networkManager.GetComponent<UnityTransport>();

        transport.SetHostRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerName",
                        new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Public,
                            playerName
                        )
                    }
                }
            },

            Data = new Dictionary<string, DataObject>
            {
                {
                    "RoomCode",
                    new DataObject(
                        DataObject.VisibilityOptions.Public,
                        roomCode
                    )
                },
                {
                    "RelayCode",
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        relayJoinCode
                    )
                }
            }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(
            roomCode,
            MaxPlayers,
            options
        );

        networkManager.StartHost();

        _ = RefreshLobbyRoutine();

        return currentLobby;
    }

    public async Task JoinRoom(string playerName, string roomCode)
    {
        await ServiceLocator.Get<NetworkServices>().Initialize();

        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    QueryFilter.FieldOptions.Name,
                    roomCode,
                    QueryFilter.OpOptions.EQ
                )
            }
        };

        QueryResponse response =
            await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

        if (response.Results.Count == 0)
            throw new System.Exception("Room introuvable.");

        Lobby lobby = response.Results[0];

        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
        {
            Player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerName",
                        new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Public,
                            playerName
                        )
                    }
                }
            }
        };

        currentLobby =
            await LobbyService.Instance.JoinLobbyByIdAsync(
                lobby.Id,
                joinOptions
            );

        string relayJoinCode =
            currentLobby.Data["RelayCode"].Value;

        JoinAllocation allocation =
            await RelayService.Instance.JoinAllocationAsync(
                relayJoinCode
            );

        UnityTransport transport =
            networkManager.GetComponent<UnityTransport>();

        transport.SetClientRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData
        );

        networkManager.StartClient();

        _ = RefreshLobbyRoutine();
    }

    public async Task LeaveRoom()
    {
        if (currentLobby == null)
            return;

        string lobbyId = currentLobby.Id;

        NetworkManager.Singleton.Shutdown();

        if (NetworkManager.Singleton.IsHost)
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
        else
        {
            await LobbyService.Instance.RemovePlayerAsync(
                lobbyId,
                AuthenticationService.Instance.PlayerId
            );
        }

        currentLobby = null;
    }

    private string GenerateRoomCode()
    {
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] code = new char[4];

        for (int i = 0; i < code.Length; i++)
            code[i] = characters[Random.Range(0, characters.Length)];

        return new string(code);
    }

    private async Task RefreshLobbyRoutine()
    {
        while (currentLobby != null)
        {
            try
            {
                currentLobby =
                    await LobbyService.Instance.GetLobbyAsync(
                        currentLobby.Id
                    );

                RoomPanel roomPanel =
                    ServiceLocator.Get<RoomPanel>();

                roomPanel.SetPlayers(
                    currentLobby.Players
                );
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                break;
            }

            await Task.Delay(1000);
        }
    }

    public string GetLocalPlayerName()
    {
        if (currentLobby == null)
            return null;

        string playerId =
            AuthenticationService.Instance.PlayerId;

        Player player =
            currentLobby.Players.Find(
                player => player.Id == playerId
            );

        if (player == null)
            return null;

        if (!player.Data.TryGetValue(
            "PlayerName",
            out PlayerDataObject playerNameData))
            return null;

        return playerNameData.Value;
    }

    public int PlayerCount =>
        currentLobby != null
            ? currentLobby.Players.Count
            : 0;

    public Lobby CurrentLobby =>
        currentLobby;
}
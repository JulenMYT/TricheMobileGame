using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class OnlineGame : NetworkBehaviour
{
    public const string NotEnoughPlayersMessage = "Il faut au moins 3 joueurs !";
    public const string PlayersNotReadyMessage = "Attends que tous les joueurs soient connectés.";
    public const string NoVoteMessage = "Personne n'a encore voté !";

    [SerializeField] private OnlineGamePanel gamePanel;
    [SerializeField] private Options options;

    [SerializeField] private int oddsSecondFake;
    [SerializeField] private int oddsAllFake;
    [SerializeField] private int minPlayers = 3;

    // Noms des joueurs connectés, enregistrés auprès du serveur.
    private readonly Dictionary<ulong, string> playerNames = new();

    // Manche en cours (serveur uniquement). Les imposteurs sont identifiés par leur
    // identifiant réseau et non par leur position, qui change si quelqu'un se déconnecte.
    private readonly HashSet<ulong> fakeClientIds = new();
    private readonly Dictionary<ulong, string> roundPlayers = new();
    private readonly Dictionary<ulong, ulong> playerVotes = new();

    private bool roundInProgress;
    private bool voteEnded;

    private string category;
    private string word;
    private string fakeWord;

    private void Awake()
    {
        ServiceLocator.Register<OnlineGame>(this);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

        RegisterLocalPlayer();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null)
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

        playerNames.Clear();
        ResetRound();
    }

    // ------------------------------------------------------------------ lancement

    // Lance une manche. Renvoie un message d'erreur à afficher à l'hôte, ou null.
    public string StartGame()
    {
        if (!IsServer)
            return null;

        List<ulong> players = GetConnectedPlayers();

        if (players.Count < minPlayers)
            return NotEnoughPlayersMessage;

        if (players.Any(id => !playerNames.ContainsKey(id)))
            return PlayersNotReadyMessage;

        ResetRound();

        foreach (ulong id in players)
            roundPlayers[id] = playerNames[id];

        AssignWords();
        AssignFakes(players);

        roundInProgress = true;

        SendPlayerWords(players);
        SendPlayersToClients(players);

        return null;
    }

    private void ResetRound()
    {
        fakeClientIds.Clear();
        roundPlayers.Clear();
        playerVotes.Clear();

        roundInProgress = false;
        voteEnded = false;

        category = null;
        word = null;
        fakeWord = null;
    }

    private List<ulong> GetConnectedPlayers()
    {
        return NetworkManager.Singleton
            .ConnectedClientsIds
            .OrderBy(id => id)
            .ToList();
    }

    private void AssignWords()
    {
        (category, word) = WordsDatabase.GetRandomWord(
            CategorySettings.GetEnabledCategoryNames(),
            options.AllowShuffle()
        );

        fakeWord = null;

        if (!options.IsUndercoverMode())
            return;

        // Comme en offline : sans autre mot dans la catégorie, la manche se joue en classique.
        fakeWord = WordsDatabase
            .GetWordCategory(category)
            ?.GetRandomWordExcept(word);
    }

    private bool IsUndercoverRound() => fakeWord != null;

    private void AssignFakes(List<ulong> players)
    {
        if (ShouldMakeAllPlayersFake())
        {
            fakeClientIds.UnionWith(players);
            return;
        }

        AddRandomFake(players);

        if (ShouldAddSecondFake(players.Count))
            AddRandomFake(players);
    }

    private bool ShouldMakeAllPlayersFake()
    {
        return options.AllowAllFake()
            && !IsUndercoverRound()
            && Random.Range(0, 100) < oddsAllFake;
    }

    private bool ShouldAddSecondFake(int playerCount)
    {
        return options.AllowSecondFake()
            && !IsUndercoverRound()
            && playerCount > 3
            && Random.Range(0, 100) < oddsSecondFake;
    }

    private void AddRandomFake(List<ulong> players)
    {
        List<ulong> candidates = players.Where(id => !fakeClientIds.Contains(id)).ToList();

        if (candidates.Count > 0)
            fakeClientIds.Add(candidates[Random.Range(0, candidates.Count)]);
    }

    private void SendPlayerWords(List<ulong> players)
    {
        foreach (ulong playerId in players)
        {
            bool isFake = fakeClientIds.Contains(playerId);

            SendWordClientRpc(
                GetPlayerWord(isFake),
                category,
                isFake && !IsUndercoverRound(),
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { playerId }
                    }
                }
            );
        }
    }

    private string GetPlayerWord(bool isFake)
    {
        if (!isFake)
            return word;

        if (IsUndercoverRound())
            return fakeWord;

        return "IMPOSTEUR";
    }

    [ClientRpc]
    private void SendWordClientRpc(
        string playerWord,
        string playerCategory,
        bool isImpostor,
        ClientRpcParams clientRpcParams = default)
    {
        gamePanel.Setup(
            playerWord,
            playerCategory,
            isImpostor
        );

        gamePanel.SetHost(IsServer);

        ServiceLocator
            .Get<MenuManager>()
            .ShowMenu(gamePanel);
    }

    // ------------------------------------------------------------------ joueurs

    private void RegisterLocalPlayer()
    {
        RoomService roomService = ServiceLocator.Get<RoomService>();
        Lobby lobby = roomService.CurrentLobby;

        if (lobby == null)
            return;

        string playerId = AuthenticationService.Instance.PlayerId;
        Player player = lobby.Players.Find(lobbyPlayer => lobbyPlayer.Id == playerId);

        if (player == null)
            return;

        if (!player.Data.TryGetValue("PlayerName", out PlayerDataObject playerName))
            return;

        RegisterPlayerServerRpc(playerName.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterPlayerServerRpc(
        string playerName,
        ServerRpcParams rpcParams = default)
    {
        playerNames[rpcParams.Receive.SenderClientId] = playerName;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Sans ça, un joueur parti restait enregistré et bloquait les parties suivantes.
        playerNames.Remove(clientId);

        if (!roundInProgress)
            return;

        // Ses votes, et les votes contre lui, ne comptent plus.
        playerVotes.Remove(clientId);

        foreach (ulong voter in playerVotes.Where(pair => pair.Value == clientId).Select(pair => pair.Key).ToList())
            playerVotes.Remove(voter);

        RemovePlayerClientRpc(clientId);
    }

    private void SendPlayersToClients(List<ulong> players)
    {
        ClearPlayersClientRpc();

        foreach (ulong playerId in players)
            AddPlayerClientRpc(playerId, roundPlayers[playerId]);
    }

    [ClientRpc]
    private void ClearPlayersClientRpc()
    {
        gamePanel.ClearPlayers();
    }

    [ClientRpc]
    private void AddPlayerClientRpc(ulong playerId, string playerName)
    {
        gamePanel.AddPlayer(playerId, playerName, playerId == NetworkManager.LocalClientId);
    }

    [ClientRpc]
    private void RemovePlayerClientRpc(ulong playerId)
    {
        gamePanel.RemovePlayer(playerId);
    }

    // ------------------------------------------------------------------ votes

    public void Vote(ulong targetPlayerId)
    {
        if (!IsClient)
            return;

        VoteServerRpc(targetPlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void VoteServerRpc(
        ulong targetPlayerId,
        ServerRpcParams rpcParams = default)
    {
        if (!roundInProgress || voteEnded)
            return;

        ulong voterPlayerId = rpcParams.Receive.SenderClientId;

        if (!roundPlayers.ContainsKey(voterPlayerId) || !roundPlayers.ContainsKey(targetPlayerId))
            return;

        if (voterPlayerId == targetPlayerId)
            return;

        playerVotes[voterPlayerId] = targetPlayerId;
    }

    // Termine le vote. Renvoie un message d'erreur à afficher à l'hôte, ou null.
    public string EndVote()
    {
        if (!IsServer || !roundInProgress || voteEnded)
            return null;

        if (playerVotes.Count == 0)
            return NoVoteMessage;

        voteEnded = true;

        ResolveVotes();

        return null;
    }

    private void ResolveVotes()
    {
        Dictionary<ulong, int> voteCounts = new();

        foreach (ulong targetPlayerId in playerVotes.Values)
        {
            voteCounts.TryGetValue(targetPlayerId, out int count);
            voteCounts[targetPlayerId] = count + 1;
        }

        int highestVotes = voteCounts.Values.Max();

        List<ulong> candidates = voteCounts
            .Where(pair => pair.Value == highestVotes)
            .Select(pair => pair.Key)
            .ToList();

        ulong votedPlayer = candidates[Random.Range(0, candidates.Count)];

        RevealResultClientRpc(
            roundPlayers[votedPlayer],
            fakeClientIds.Contains(votedPlayer),
            GetImpostorNames(),
            word
        );

        playerVotes.Clear();
    }

    private string GetImpostorNames()
    {
        // Les noms viennent de la manche : un imposteur parti entre-temps est quand même affiché.
        return string.Join(", ", fakeClientIds
            .OrderBy(id => id)
            .Where(roundPlayers.ContainsKey)
            .Select(id => roundPlayers[id]));
    }

    [ClientRpc]
    private void RevealResultClientRpc(
        string votedPlayerName,
        bool votedPlayerIsImpostor,
        string impostorPlayerName,
        string realWord)
    {
        gamePanel.ShowReveal(
            votedPlayerName,
            votedPlayerIsImpostor,
            impostorPlayerName,
            realWord
        );
    }

    // ------------------------------------------------------------------ fin de manche

    // L'hôte ramène tout le monde dans le salon, d'où il peut relancer une manche.
    public void BackToRoom()
    {
        if (!IsServer)
            return;

        ResetRound();
        BackToRoomClientRpc();
    }

    [ClientRpc]
    private void BackToRoomClientRpc()
    {
        ServiceLocator
            .Get<MenuManager>()
            .ShowMenu(ServiceLocator.Get<RoomPanel>());
    }
}

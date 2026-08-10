using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class OnlineGame : NetworkBehaviour
{
    [SerializeField] private OnlineGamePanel gamePanel;
    [SerializeField] private Options options;

    [SerializeField] private int oddsSecondFake;
    [SerializeField] private int oddsAllFake;

    private readonly List<int> fakePlayerIndices = new();
    private readonly Dictionary<ulong, string> playerNames = new();
    private readonly Dictionary<ulong, ulong> playerVotes = new();

    private string category;
    private string word;
    private string fakeWord;

    private void Awake()
    {
        ServiceLocator.Register<OnlineGame>(this);
    }

    public override void OnNetworkSpawn()
    {
        RegisterLocalPlayer();
    }

    public void StartGame()
    {
        if (!IsServer)
            return;

        ResetGame();

        List<ulong> players = GetConnectedPlayers();

        if (players.Count < 1)
        {
            Debug.LogWarning("Impossible de lancer la partie : il faut au moins 3 joueurs.");
            return;
        }

        if (playerNames.Count != players.Count)
        {
            Debug.LogWarning("Impossible de lancer la partie : tous les joueurs ne sont pas encore enregistrés.");
            return;
        }

        AssignWords();
        AssignFakes(players.Count);
        SendPlayerWords(players);
        SendPlayersToClients(players);
    }

    private void ResetGame()
    {
        fakePlayerIndices.Clear();
        playerVotes.Clear();

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

        do
        {
            fakeWord = WordsDatabase
                .GetWordCategory(category)
                .GetRandomWord();

        } while (fakeWord == word);
    }

    private void AssignFakes(int playerCount)
    {
        fakePlayerIndices.Clear();

        if (ShouldMakeAllPlayersFake())
        {
            for (int i = 0; i < playerCount; i++)
                fakePlayerIndices.Add(i);

            return;
        }

        AddRandomFake(playerCount);

        if (ShouldAddSecondFake(playerCount))
            AddRandomFake(playerCount);
    }

    private bool ShouldMakeAllPlayersFake()
    {
        return options.AllowAllFake()
            && !options.IsUndercoverMode()
            && Random.Range(0, 100) < oddsAllFake;
    }

    private bool ShouldAddSecondFake(int playerCount)
    {
        return options.AllowSecondFake()
            && !options.IsUndercoverMode()
            && playerCount > 3
            && Random.Range(0, 100) < oddsSecondFake;
    }

    private void AddRandomFake(int playerCount)
    {
        int fakeIndex;

        do
        {
            fakeIndex = Random.Range(0, playerCount);
        }
        while (fakePlayerIndices.Contains(fakeIndex));

        fakePlayerIndices.Add(fakeIndex);
    }

    private void SendPlayerWords(List<ulong> players)
    {
        for (int i = 0; i < players.Count; i++)
        {
            bool isFake = fakePlayerIndices.Contains(i);
            string playerWord = GetPlayerWord(isFake);

            SendWordClientRpc(
                playerWord,
                category,
                isFake && !options.IsUndercoverMode(),
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { players[i] }
                    }
                }
            );
        }
    }

    private string GetPlayerWord(bool isFake)
    {
        if (!isFake)
            return word;

        if (options.IsUndercoverMode())
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
            .ToggleMenu(gamePanel);
    }

    private void RegisterLocalPlayer()
    {
        RoomService roomService =
            ServiceLocator.Get<RoomService>();

        Lobby lobby =
            roomService.CurrentLobby;

        if (lobby == null)
            return;

        string playerId =
            AuthenticationService.Instance.PlayerId;

        Player player = lobby.Players.Find(
            lobbyPlayer => lobbyPlayer.Id == playerId
        );

        if (player == null)
            return;

        if (!player.Data.TryGetValue(
            "PlayerName",
            out PlayerDataObject playerName))
            return;

        RegisterPlayerServerRpc(
            playerName.Value
        );
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterPlayerServerRpc(
        string playerName,
        ServerRpcParams rpcParams = default)
    {
        ulong clientId =
            rpcParams.Receive.SenderClientId;

        playerNames[clientId] = playerName;
    }

    private void SendPlayersToClients(List<ulong> players)
    {
        ClearPlayersClientRpc();

        foreach (ulong playerId in players)
        {
            if (!playerNames.TryGetValue(
                playerId,
                out string playerName))
                continue;

            AddPlayerClientRpc(
                playerId,
                playerName
            );
        }
    }

    [ClientRpc]
    private void ClearPlayersClientRpc()
    {
        gamePanel.ClearPlayers();
    }

    [ClientRpc]
    private void AddPlayerClientRpc(
        ulong playerId,
        string playerName)
    {
        gamePanel.AddPlayer(
            playerId,
            playerName
        );
    }

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
        ulong voterPlayerId =
            rpcParams.Receive.SenderClientId;

        if (!playerNames.ContainsKey(voterPlayerId))
            return;

        if (!playerNames.ContainsKey(targetPlayerId))
            return;

        if (voterPlayerId == targetPlayerId)
            return;

        playerVotes[voterPlayerId] = targetPlayerId;
    }

    private bool voteEnded;

    public void EndVote()
    {
        if (!IsServer)
            return;

        if (voteEnded)
            return;

        voteEnded = true;

        ResolveVotes();
    }

    private void ResolveVotes()
    {
        if (playerVotes.Count == 0)
        {
            voteEnded = false;
            return;
        }

        Dictionary<ulong, int> voteCounts = new();

        foreach (ulong targetPlayerId in playerVotes.Values)
        {
            if (!voteCounts.ContainsKey(targetPlayerId))
                voteCounts[targetPlayerId] = 0;

            voteCounts[targetPlayerId]++;
        }

        int highestVotes =
            voteCounts.Values.Max();

        List<ulong> candidates =
            voteCounts
                .Where(pair => pair.Value == highestVotes)
                .Select(pair => pair.Key)
                .ToList();

        ulong votedPlayer =
            candidates[Random.Range(0, candidates.Count)];

        bool votedPlayerIsImpostor =
            IsPlayerImpostor(votedPlayer);

        List<ulong> players =
            GetConnectedPlayers();

        string impostorPlayerName =
            GetImpostorNames(players);

        RevealResultClientRpc(
            playerNames[votedPlayer],
            votedPlayerIsImpostor,
            impostorPlayerName,
            word
        );

        playerVotes.Clear();
    }

    private bool IsPlayerImpostor(ulong playerId)
    {
        List<ulong> players = GetConnectedPlayers();

        int index = players.IndexOf(playerId);

        return fakePlayerIndices.Contains(index);
    }

    private string GetImpostorNames(List<ulong> players)
    {
        List<string> impostorNames = new();

        foreach (int fakeIndex in fakePlayerIndices)
        {
            if (fakeIndex < 0 || fakeIndex >= players.Count)
                continue;

            ulong playerId = players[fakeIndex];

            if (playerNames.TryGetValue(playerId, out string playerName))
                impostorNames.Add(playerName);
        }

        return string.Join(", ", impostorNames);
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
}
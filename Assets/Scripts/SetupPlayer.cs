using System.Collections.Generic;
using UnityEngine;

public class SetupPlayer : MenuPanel
{
    private const string EmptyNameMessage = "Un joueur n'a pas de nom !";
    private const string DupeNameMessage = "Les joueurs doivent avoir des noms différents !";
    private const string LowPlayerMessage = "Il faut au moins 3 joueurs !";
    private const string NoCategoryMessage = "Active au moins une catégorie !";

    [SerializeField] private RectTransform gridParent;
    [SerializeField] private PlayerBox playerBoxPrefab;
    [SerializeField] private ErrorPopup errorPopupPrefab;

    // Noms de la dernière partie, séparés par des retours à la ligne.
    private const string SavedPlayersKey = "Players_Names";
    private const int DefaultPlayerCount = 3;

    public List<PlayerBox> playerBoxes { get; private set; } = new();

    private void Start()
    {
        RestorePlayers();
    }

    public void AddPlayer()
    {
        PlayerBox player = CreatePlayerBox(string.Empty);

        UIUtility.ScrollToBottom(gridParent);
        player.Focus();
    }

    private PlayerBox CreatePlayerBox(string playerName)
    {
        PlayerBox player = Instantiate(playerBoxPrefab, gridParent);
        player.Initialize(this);
        player.SetName(playerName);
        playerBoxes.Add(player);

        return player;
    }

    // Remet les joueurs de la dernière partie, ou 3 cases vides au premier lancement,
    // pour ne pas avoir à tout ressaisir à chaque fois.
    private void RestorePlayers()
    {
        if (playerBoxes.Count > 0)
            return;

        string saved = PlayerPrefs.GetString(SavedPlayersKey, string.Empty);
        string[] names = saved.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

        if (names.Length == 0)
        {
            for (int i = 0; i < DefaultPlayerCount; i++)
                CreatePlayerBox(string.Empty);

            return;
        }

        foreach (string playerName in names)
            CreatePlayerBox(playerName);
    }

    private void SavePlayers(List<string> players)
    {
        PlayerPrefs.SetString(SavedPlayersKey, string.Join("\n", players));
        PlayerPrefs.Save();
    }

    public void RemovePlayer(PlayerBox player)
    {
        if (!playerBoxes.Remove(player))
            return;

        Destroy(player.gameObject);
    }

    public bool IsValid()
    {
        if (playerBoxes.Count < 3)
        {
            ShowError(LowPlayerMessage);
            return false;
        }

        if (CategorySettings.GetEnabledCount() == 0)
        {
            ShowError(NoCategoryMessage);
            return false;
        }

        HashSet<string> names = new();

        foreach (PlayerBox player in playerBoxes)
        {
            string name = player.GetName().Trim();

            if (string.IsNullOrEmpty(name))
            {
                ShowError(EmptyNameMessage);
                return false;
            }

            if (!names.Add(name.ToLowerInvariant()))
            {
                ShowError(DupeNameMessage);
                return false;
            }
        }

        return true;
    }

    public List<string> GetPlayers()
    {
        List<string> players = new();

        foreach (PlayerBox player in playerBoxes)
            players.Add(player.GetName().Trim());

        SavePlayers(players);

        return players;
    }

    private void ShowError(string message)
    {
        ErrorPopup popup = Instantiate(errorPopupPrefab);
        popup.Setup(message);
    }
}
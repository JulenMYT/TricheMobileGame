using System.Collections.Generic;
using UnityEngine;

public class SetupPlayer : MenuPanel
{
    private const string EmptyNameMessage = "Un joueur n'a pas de nom !";
    private const string DupeNameMessage = "Les joueurs doivent avoir des noms différents !";
    private const string LowPlayerMessage = "Il faut au moins 3 joueurs !";

    [SerializeField] private RectTransform gridParent;
    [SerializeField] private PlayerBox playerBoxPrefab;
    [SerializeField] private ErrorPopup errorPopupPrefab;

    public List<PlayerBox> playerBoxes { get; private set; } = new();

    public void AddPlayer()
    {
        PlayerBox player = Instantiate(playerBoxPrefab, gridParent);
        player.Initialize(this);
        playerBoxes.Add(player);
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

        return players;
    }

    private void ShowError(string message)
    {
        ErrorPopup popup = Instantiate(errorPopupPrefab);
        popup.Setup(message);
    }
}
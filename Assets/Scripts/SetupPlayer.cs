using System.Collections.Generic;
using UnityEngine;

public class SetupPlayer : MenuPanel
{
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private PlayerBox playerBoxPrefab;

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
            return false;

        HashSet<string> names = new();

        foreach (PlayerBox player in playerBoxes)
        {
            string name = player.GetName().Trim();

            if (string.IsNullOrEmpty(name))
                return false;

            if (!names.Add(name.ToLowerInvariant()))
                return false;
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
}
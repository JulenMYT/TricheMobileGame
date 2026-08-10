using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class RoomPanel : MenuPanel
{
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private PlayerCard playerCardPrefab;
    [SerializeField] private GameObject startButton;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private ErrorPopup errorPopup;

    private MenuManager menuManager;

    private bool isHost;

    private void Awake()
    {
        ServiceLocator.Register<RoomPanel>(this);
    }

    private void Start()
    {
        menuManager = ServiceLocator.Get<MenuManager>();
    }

    public void SetHost(bool value)
    {
        isHost = value;
        startButton.SetActive(isHost);
    }

    public void SetRoomCode(string roomCode)
    {
        roomCodeText.text = roomCode;
    }

    public void SetPlayers(IReadOnlyList<Player> players)
    {
        ClearPlayers();

        foreach (Player player in players)
        {
            if (!player.Data.TryGetValue(
                "PlayerName",
                out PlayerDataObject playerName))
                continue;

            AddPlayer(playerName.Value);
        }
    }

    private void AddPlayer(string playerName)
    {
        PlayerCard playerCard =
            Instantiate(playerCardPrefab, playersContainer);

        playerCard.Setup(playerName);
    }

    public void ClearPlayers()
    {
        foreach (Transform child in playersContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    public void StartGame()
    {
        if (!isHost)
            return;

        ServiceLocator.Get<OnlineGame>().StartGame();
    }

    public override void Close()
    {
        ClearPlayers();
    }

    public async void Leave()
    {
        await ServiceLocator.Get<RoomService>().LeaveRoom();

        menuManager.ToggleMenu(mainMenu);
    }
}
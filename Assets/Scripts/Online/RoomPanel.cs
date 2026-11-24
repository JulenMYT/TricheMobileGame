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
    private RoomService roomService;

    private bool isHost;
    private bool leaving;

    private void Awake()
    {
        ServiceLocator.Register<RoomPanel>(this);
    }

    private void Start()
    {
        menuManager = ServiceLocator.Get<MenuManager>();
        roomService = ServiceLocator.Get<RoomService>();

        roomService.Disconnected += OnDisconnected;
    }

    private void OnDestroy()
    {
        if (roomService != null)
            roomService.Disconnected -= OnDisconnected;
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

    public override void Open()
    {
        // Affiche tout de suite les joueurs connus, sans attendre la prochaine mise à jour.
        if (roomService != null && roomService.CurrentLobby != null)
            SetPlayers(roomService.CurrentLobby.Players);
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

        string error = ServiceLocator.Get<OnlineGame>().StartGame();

        if (error != null)
            ErrorPopup.Show(errorPopup, error);
    }

    public override void Close()
    {
        ClearPlayers();
    }

    public async void Leave()
    {
        if (leaving)
            return;

        leaving = true;

        try
        {
            await roomService.LeaveRoom();
        }
        catch (System.Exception exception)
        {
            // Même en cas d'erreur réseau, on revient au menu : le salon expirera seul.
            Debug.LogException(exception);
        }
        finally
        {
            leaving = false;
            menuManager.ShowMenu(mainMenu);
        }
    }

    // L'hôte est parti, le salon a été fermé ou la connexion a été perdue.
    private void OnDisconnected(string message)
    {
        ErrorPopup.Show(errorPopup, message);
        menuManager.ShowMenu(mainMenu);
    }
}

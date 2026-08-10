using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class HostPanel : MenuPanel
{
    private const string EmptyNameError = "Veuillez entrer un nom.";

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private ErrorPopup errorPopup;
    [SerializeField] private RoomPanel roomPanel;

    private RoomService roomService;
    private MenuManager menuManager;

    private void Start()
    {
        roomService = ServiceLocator.Get<RoomService>();
        menuManager = ServiceLocator.Get<MenuManager>();
    }

    public async void CreateGame()
    {
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            ErrorPopup popup = Instantiate(errorPopup);
            popup.Setup(EmptyNameError);
            return;
        }

        try
        {
            Lobby lobby = await roomService.CreateRoom(playerName);

            string roomCode = lobby.Data["RoomCode"].Value;

            roomPanel.SetRoomCode(roomCode);
            roomPanel.SetHost(true);

            menuManager.ToggleMenu(roomPanel);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
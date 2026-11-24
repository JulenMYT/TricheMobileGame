using System;
using TMPro;
using UnityEngine;

public class JoinPanel : MenuPanel
{
    private const string EmptyNameError = "Veuillez entrer un nom.";
    private const string EmptyRoomCodeError = "Veuillez entrer un code.";
    private const string ConnectingMessage = "Connexion au salon...";

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private ErrorPopup errorPopup;
    [SerializeField] private RoomPanel roomPanel;

    private RoomService roomService;
    private MenuManager menuManager;

    // Empêche de rejoindre deux fois en appuyant deux fois pendant l'attente.
    private bool busy;

    private void Start()
    {
        roomService = ServiceLocator.Get<RoomService>();
        menuManager = ServiceLocator.Get<MenuManager>();
    }

    public override void Open()
    {
        OnlinePlayerName.Fill(playerNameInput);
    }

    public async void JoinGame()
    {
        if (busy)
            return;

        string playerName = playerNameInput.text.Trim();
        string roomCode = roomCodeInput.text.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(playerName))
        {
            ErrorPopup.Show(errorPopup, EmptyNameError);
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            ErrorPopup.Show(errorPopup, EmptyRoomCodeError);
            return;
        }

        busy = true;
        ErrorPopup.Show(errorPopup, ConnectingMessage);

        try
        {
            await roomService.JoinRoom(playerName, roomCode);

            OnlinePlayerName.Save(playerName);

            roomPanel.SetRoomCode(roomService.RoomCode);
            roomPanel.SetHost(false);

            menuManager.ShowMenu(roomPanel);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ErrorPopup.Show(errorPopup, RoomService.GetErrorMessage(exception));
        }
        finally
        {
            busy = false;
        }
    }
}

using System;
using TMPro;
using UnityEngine;

public class HostPanel : MenuPanel
{
    private const string EmptyNameError = "Veuillez entrer un nom.";
    private const string ConnectingMessage = "Création du salon...";

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private ErrorPopup errorPopup;
    [SerializeField] private RoomPanel roomPanel;

    private RoomService roomService;
    private MenuManager menuManager;

    // Empêche de créer deux salons en appuyant deux fois pendant l'attente.
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

    public async void CreateGame()
    {
        if (busy)
            return;

        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            ErrorPopup.Show(errorPopup, EmptyNameError);
            return;
        }

        busy = true;
        ErrorPopup.Show(errorPopup, ConnectingMessage);

        try
        {
            await roomService.CreateRoom(playerName);

            OnlinePlayerName.Save(playerName);

            roomPanel.SetRoomCode(roomService.RoomCode);
            roomPanel.SetHost(true);

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

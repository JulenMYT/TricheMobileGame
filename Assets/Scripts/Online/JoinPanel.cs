using TMPro;
using UnityEngine;

public class JoinPanel : MenuPanel
{
    private const string EmptyNameError = "Veuillez entrer un nom.";
    private const string EmptyRoomCodeError = "Veuillez entrer un code.";

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private ErrorPopup errorPopup;
    [SerializeField] private RoomPanel roomPanel;

    private RoomService roomService;
    private MenuManager menuManager;

    private void Start()
    {
        roomService = ServiceLocator.Get<RoomService>();
        menuManager = ServiceLocator.Get<MenuManager>();
    }

    public async void JoinGame()
    {
        string playerName = playerNameInput.text.Trim();
        string roomCode = roomCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(playerName))
        {
            ErrorPopup popup = Instantiate(errorPopup);
            popup.Setup(EmptyNameError);
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            ErrorPopup popup = Instantiate(errorPopup);
            popup.Setup(EmptyRoomCodeError);
            return;
        }

        try
        {
            await roomService.JoinRoom(playerName, roomCode);

            roomPanel.SetRoomCode(roomCode);
            roomPanel.SetHost(false);

            menuManager.ToggleMenu(roomPanel);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
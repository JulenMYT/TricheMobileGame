using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlinePlayerButton : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image buttonImage;

    private ulong playerId;
    private OnlineGamePanel gamePanel;
    private Color baseColor;

    private void Awake()
    {
        baseColor = buttonImage.color;
    }

    public void Setup(
        ulong id,
        string playerName,
        OnlineGamePanel panel)
    {
        playerId = id;
        playerNameText.text = playerName;
        gamePanel = panel;
    }

    public ulong GetPlayerId()
    {
        return playerId;
    }

    public void Vote()
    {
        gamePanel.SelectPlayer(playerId);

        ServiceLocator
            .Get<OnlineGame>()
            .Vote(playerId);
    }

    public void SetSelected(bool selected)
    {
        buttonImage.color = selected
            ? Color.yellow
            : baseColor;
    }
}
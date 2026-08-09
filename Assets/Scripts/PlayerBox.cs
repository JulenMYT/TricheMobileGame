using TMPro;
using UnityEngine;

public class PlayerBox : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private SetupPlayer setupPlayer;

    public void Initialize(SetupPlayer setupPlayer)
    {
        this.setupPlayer = setupPlayer;
    }

    public string GetName()
    {
        return inputField.text;
    }

    public void DeleteBox()
    {
        setupPlayer.RemovePlayer(this);
    }
}
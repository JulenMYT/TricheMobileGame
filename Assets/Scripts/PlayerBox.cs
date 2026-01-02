using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBox : MonoBehaviour
{
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_InputField inputField;

    public event Action<PlayerBox, string> OnNameEdited;
    public event Action<PlayerBox> OnDeleteClicked;

    private void Awake()
    {
        deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(this));
        inputField.onEndEdit.AddListener(value => OnNameEdited?.Invoke(this, value));
    }

    public string GetName()
    {
        return inputField.text;
    }

    public void SetName(string value)
    {
        inputField.SetTextWithoutNotify(value);
    }
}

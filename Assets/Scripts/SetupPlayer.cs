using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupPlayer : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private PlayerBox playerBoxPrefab;
    [SerializeField] private Button addButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;

    public List<PlayerBox> playerBoxes { get; private set; } = new();

    public event Action OnPlayClicked;
    public event Action OnOptionsClicked;

    private void Awake()
    {
        addButton.onClick.AddListener(AddPlayer);
        playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
        optionsButton.onClick.AddListener(() => OnOptionsClicked?.Invoke());
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void AddPlayer()
    {
        PlayerBox player = Instantiate(playerBoxPrefab, gridParent);
        player.OnDeleteClicked += DeleteBox;
        player.OnNameEdited += HandleNameEdited;
        playerBoxes.Add(player);
    }

    private void DeleteBox(PlayerBox box)
    {
        playerBoxes.Remove(box);
        Destroy(box.gameObject);
    }

    public void DeleteEmptyBoxes()
    {
        for (int i = playerBoxes.Count - 1; i >= 0; i--)
        {
            if (string.IsNullOrWhiteSpace(playerBoxes[i].GetName()))
                DeleteBox(playerBoxes[i]);
        }
    }

    public void ForceValidate()
    {
        foreach(PlayerBox box in playerBoxes)
            HandleNameEdited(box, box.GetName());
    }

    private void HandleNameEdited(PlayerBox source, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return;

        string baseName = newName.Trim();
        int maxSuffix = 0;

        foreach (var box in playerBoxes)
        {
            if (box == source)
                continue;

            string existing = box.GetName();

            if (existing == baseName)
            {
                maxSuffix = Mathf.Max(maxSuffix, 1);
            }
            else if (existing.StartsWith(baseName))
            {
                string suffixStr = existing.Substring(baseName.Length);
                if (int.TryParse(suffixStr, out int suffix))
                    maxSuffix = Mathf.Max(maxSuffix, suffix);
            }
        }

        if (maxSuffix > 0)
            source.SetName(baseName + (maxSuffix + 1));
    }
}

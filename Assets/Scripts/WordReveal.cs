using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordReveal : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private TMP_Text wordText;

    [SerializeField] private GameObject clickImage;
    [SerializeField] private Button clickButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private Color legitColour;
    [SerializeField] private Color fakeColour;

    public event Action OnNextButtonClicked;

    private void Awake()
    {
        clickButton.onClick.AddListener(() => { HideClickImage(); EnableNextButton(); });
        nextButton.onClick.AddListener(() => {OnNextButtonClicked?.Invoke(); DisableNextButton(); });
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

    public void Setup(string category, string word, string player, bool fake)
    {
        categoryText.text = category;
        playerName.text = player;
        wordText.text = word;
        wordText.color = fake ? fakeColour : legitColour;

        ShowClickImage();
        DisableNextButton();
    }

    private void ShowClickImage()
    {
        clickImage.SetActive(true);
    }

    private void HideClickImage()
    {
        clickImage?.SetActive(false);
    }

    private void EnableNextButton()
    {
        nextButton.gameObject.SetActive(true);
    }

    private void DisableNextButton()
    {
        nextButton?.gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private HandleCategory handleCategory;

    [SerializeField] private Toggle twoFakeToggle;
    [SerializeField] private Toggle allFakeToggle;
    [SerializeField] private Toggle fakeStartToggle;
    [SerializeField] private Toggle shuffleToggle;

    [SerializeField] private Button handleCategoryButton;

    [SerializeField] private Button validateButton;

    public event Action OnValidateButton;

    private void Awake()
    {
        validateButton.onClick.AddListener(() => OnValidateButton?.Invoke());
        handleCategoryButton.onClick.AddListener(HandleCategory);
        handleCategory.OnValidateButton += CloseCategory;
    }

    private void Start()
    {
        handleCategory.Hide();
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

    public bool AllowSecondFake()
    {
        return twoFakeToggle.isOn;
    }

    public bool AllowAllFake()
    {
        return allFakeToggle.isOn;
    }

    public bool AllowFakeStart()
    {
        return fakeStartToggle.isOn;
    }

    public bool AllowShuffle()
    {
        return shuffleToggle.isOn;
    }

    public List<string> GetAllowedCategories()
    {
        return CategorySettings.GetEnabledCategoryNames();
    }

    private void HandleCategory()
    {
        handleCategory.Show();
        Hide();
    }

    private void CloseCategory()
    {
        handleCategory.Hide();
        Show();
    }
}

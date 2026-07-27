using System;
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
    [SerializeField] private Toggle undercoverToggle;

    [SerializeField] private Button handleCategoryButton;
    [SerializeField] private Button validateButton;

    public event Action OnValidateButton;

    private const string TWO_FAKE_KEY = "Options_TwoFake";
    private const string ALL_FAKE_KEY = "Options_AllFake";
    private const string FAKE_START_KEY = "Options_FakeStart";
    private const string SHUFFLE_KEY = "Options_Shuffle";
    private const string UNDERCOVER_KEY = "Options_Undercover";

    private void Start()
    {
        validateButton.onClick.AddListener(Validate);
        handleCategoryButton.onClick.AddListener(HandleCategory);
        handleCategory.OnValidateButton += CloseCategory;
        handleCategory.Hide();

        LoadSettings();
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

    public bool UndercoverMode()
    {
        return undercoverToggle.isOn;
    }

    public List<string> GetAllowedCategories()
    {
        return CategorySettings.GetEnabledCategoryNames();
    }

    private void Validate()
    {
        SaveSettings();
        OnValidateButton?.Invoke();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(TWO_FAKE_KEY, twoFakeToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(ALL_FAKE_KEY, allFakeToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(FAKE_START_KEY, fakeStartToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(SHUFFLE_KEY, shuffleToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(UNDERCOVER_KEY, undercoverToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        twoFakeToggle.isOn = PlayerPrefs.GetInt(TWO_FAKE_KEY, 0) == 1;
        allFakeToggle.isOn = PlayerPrefs.GetInt(ALL_FAKE_KEY, 0) == 1;
        fakeStartToggle.isOn = PlayerPrefs.GetInt(FAKE_START_KEY, 0) == 1;
        shuffleToggle.isOn = PlayerPrefs.GetInt(SHUFFLE_KEY, 0) == 1;
        undercoverToggle.isOn = PlayerPrefs.GetInt(UNDERCOVER_KEY, 0) == 1;
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
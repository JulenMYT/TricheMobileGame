using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryBox : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryName;
    [SerializeField] private TMP_Text number;

    [SerializeField] private Button modifyButton;
    [SerializeField] private Button deleteButton;

    [SerializeField] private Button toggle;
    [SerializeField] private Image background;

    [SerializeField] private Color enabledColour;
    [SerializeField] private Color disabledColour;

    private bool categoryEnabled;

    private WordsDatabase.WordCategory category;

    public event Action<WordsDatabase.WordCategory> OnDeleteClicked;
    public event Action<WordsDatabase.WordCategory> OnModifyClicked;

    private void Awake()
    {
        toggle.onClick.AddListener(ToggleButton);
    }

    private void Start()
    {
        deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(category));
        modifyButton.onClick.AddListener(()=>OnModifyClicked?.Invoke(category));
    }

    public void Setup(WordsDatabase.WordCategory wordCategory)
    {
        category = wordCategory;
        categoryName.text = wordCategory.categoryName;
        number.text = wordCategory.GetWordCount().ToString() + " mots";

        if (!wordCategory.IsCustom)
        {
            modifyButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
        }

        categoryEnabled = CategorySettings.IsCategoryEnabled(category.categoryName);
        RefreshVisual();
    }

    private void ToggleButton()
    {
        categoryEnabled = !categoryEnabled;
        CategorySettings.SetCategoryEnabled(category.categoryName, categoryEnabled);
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        categoryName.alpha = categoryEnabled ? 1f : 0.8f;
        number.alpha = categoryEnabled ? 1f : 0.8f;
        background.color = categoryEnabled? enabledColour : disabledColour;
    }
}

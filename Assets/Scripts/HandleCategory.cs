using System;
using UnityEngine;
using UnityEngine.UI;

public class HandleCategory : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CategoryBox prefab;
    [SerializeField] private Transform contentParent;

    [SerializeField] private Button validateButton;

    [SerializeField] private AddBox addCategoryBoxPrefab;
    private AddBox addCategoryBox;

    [SerializeField] private CategoryManager categoryManager;

    public event Action OnValidateButton;

    private void Start()
    {
        DisplayCategory();
        validateButton.onClick.AddListener(() => OnValidateButton?.Invoke());
        categoryManager.OnValidate += CloseCategoryManager;
        categoryManager.OnCancel += CloseCategoryManager;
        categoryManager.Hide();
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

    private void DisplayCategory()
    {
        if (addCategoryBox)
            addCategoryBox.OnClick -= OnAddClicked;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var categories = WordsDatabase.GetAllCategories();

        foreach (var category in categories)
        {
            var box = Instantiate(prefab, contentParent);
            box.Setup(category);
            box.OnModifyClicked += OnModifyClicked;
            box.OnDeleteClicked += OnDeleteClicked;
        }

        addCategoryBox = Instantiate (addCategoryBoxPrefab, contentParent);
        addCategoryBox.OnClick += OnAddClicked;
    }

    private void OnModifyClicked(WordsDatabase.WordCategory category)
    {
        categoryManager.Show();
        categoryManager.OpenCategory(category);
        Hide();
    }

    private void OnDeleteClicked(WordsDatabase.WordCategory category)
    {
        CustomCategoryManager.DeleteCategory(category.categoryName);
        DisplayCategory();
    }

    private void OnAddClicked()
    {
        categoryManager.Show();
        categoryManager.Reset();
        Hide();
    }

    private void CloseCategoryManager()
    {
        categoryManager.Hide();
        DisplayCategory();
        Show();
    }
}

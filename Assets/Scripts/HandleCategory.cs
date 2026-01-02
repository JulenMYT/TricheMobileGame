using System;
using UnityEngine;
using UnityEngine.UI;

public class HandleCategory : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CategoryBox prefab;
    [SerializeField] private Transform contentParent;

    [SerializeField] private Button validateButton;

    public event Action OnValidateButton;

    private void Awake()
    {
        DisplayCategory();
        validateButton.onClick.AddListener(() => OnValidateButton?.Invoke());
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
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var categories = WordsDatabase.GetAllCategories();

        foreach (var category in categories)
        {
            var box = Instantiate(prefab, contentParent);
            box.Setup(category.categoryName, category.GetWordCount());
        }
    }
}

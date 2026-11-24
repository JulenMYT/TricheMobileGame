using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryBox : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryName;
    [SerializeField] private TMP_Text number;

    [SerializeField] private Image background;

    [SerializeField] private Color enabledColour;
    [SerializeField] private Color disabledColour;

    [SerializeField] private GameObject modifyButton;
    [SerializeField] private GameObject deleteButton;

    private WordsDatabase.WordCategory category;

    private bool categoryEnabled;
    private CategoryHandler categoryHandler;

    public void Setup(
        WordsDatabase.WordCategory wordCategory,
        CategoryHandler categoryHandler)
    {
        category = wordCategory;
        this.categoryHandler = categoryHandler;

        categoryName.text = wordCategory.Name;
        number.text = $"{wordCategory.GetWordCount()} mots";

        categoryEnabled =
            CategorySettings.IsCategoryEnabled(category.Name);

        modifyButton.SetActive(category.IsCustom);
        deleteButton.SetActive(category.IsCustom);

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        float alpha = categoryEnabled ? 1f : 0.8f;

        categoryName.alpha = alpha;
        number.alpha = alpha;

        background.color = categoryEnabled
            ? enabledColour
            : disabledColour;
    }

    public void Toggle()
    {
        categoryEnabled = !categoryEnabled;

        CategorySettings.SetCategoryEnabled(
            category.Name,
            categoryEnabled
        );

        RefreshVisual();
    }

    public void Modify()
    {
        categoryHandler.EditCategory(category);
    }

    // Délai pendant lequel un deuxième appui confirme la suppression.
    private const float DeleteConfirmDelay = 3f;

    private float deleteRequestTime = float.NegativeInfinity;

    public void Delete()
    {
        if (!category.IsCustom)
            return;

        // Premier appui : on demande confirmation, la suppression est définitive.
        if (Time.unscaledTime - deleteRequestTime > DeleteConfirmDelay)
        {
            deleteRequestTime = Time.unscaledTime;
            categoryHandler.ShowMessage($"Appuie encore pour supprimer « {category.Name} »");
            return;
        }

        CustomCategoryManager.DeleteCategory(category.Name);
        categoryHandler.DisplayCategories();
    }
}
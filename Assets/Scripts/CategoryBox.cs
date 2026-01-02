using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryBox : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryName;
    [SerializeField] private TMP_Text number;

    [SerializeField] private Button toggle;
    [SerializeField] private Image background;

    [SerializeField] private Color enabledColour;
    [SerializeField] private Color disabledColour;

    private string category;
    private bool categoryEnabled;

    private void Awake()
    {
        toggle.onClick.AddListener(ToggleButton);
    }

    public void Setup(string categoryNameValue, int count)
    {
        category = categoryNameValue;
        categoryName.text = categoryNameValue;
        number.text = count.ToString() + " mots";

        categoryEnabled = CategorySettings.IsCategoryEnabled(category);
        RefreshVisual();
    }

    private void ToggleButton()
    {
        categoryEnabled = !categoryEnabled;
        CategorySettings.SetCategoryEnabled(category, categoryEnabled);
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        categoryName.alpha = categoryEnabled ? 1f : 0.8f;
        number.alpha = categoryEnabled ? 1f : 0.8f;
        background.color = categoryEnabled? enabledColour : disabledColour;
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class CategoryHandler : MenuPanel
{
    [SerializeField] private CategoryBox categoryBoxPrefab;
    [SerializeField] private Transform contentParent;

    [SerializeField] private AddCategoryBox addCategoryBoxPrefab;

    [SerializeField] private CategoryEditor categoryEditor;

    private MenuManager menuManager;

    private void Start()
    {
        menuManager = ServiceLocator.Get<MenuManager>();

        DisplayCategories();
    }

    public void DisplayCategories()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (WordsDatabase.WordCategory category in WordsDatabase.GetAllCategories())
        {
            CategoryBox box = Instantiate(categoryBoxPrefab, contentParent);

            box.Setup(category, this);
        }

        AddCategoryBox addCategoryBox = Instantiate(addCategoryBoxPrefab, contentParent);
        addCategoryBox.Setup(this);
    }

    public void EditCategory(WordsDatabase.WordCategory category)
    {
        categoryEditor.OpenCategory(category);
        menuManager.ToggleMenu(categoryEditor);
    }

    public void AddCategory()
    {
        categoryEditor.Reset();
        menuManager.ToggleMenu(categoryEditor);
    }
}

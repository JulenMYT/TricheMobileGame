using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryEditor : MenuPanel
{
    private const string EmptyTitleMessage = "La catégorie doit avoir un nom !";
    private const string EmptyWordsMessage = "La catégorie doit contenir au moins un mot !";

    [SerializeField] private CategoryHandler categoryHandler;

    [SerializeField] private Transform contentParent;
    [SerializeField] private WordBox wordBoxPrefab;

    [SerializeField] private TMP_InputField categoryTitle;
    [SerializeField] private TMP_InputField bulkInputField;

    [SerializeField] private AddWordBox addWordBoxPrefab;
    [SerializeField] private ErrorPopup errorPopupPrefab;

    private readonly List<WordBox> wordBoxes = new();

    private AddWordBox addWordBox;
    private string editingCategory;

    private MenuManager menuManager;

    private void Start()
    {
        menuManager = ServiceLocator.Get<MenuManager>();
    }

    public void Reset()
    {
        editingCategory = null;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        wordBoxes.Clear();

        categoryTitle.text = string.Empty;
        bulkInputField.text = string.Empty;

        addWordBox = Instantiate(addWordBoxPrefab, contentParent);
        addWordBox.Setup(this);
    }

    public void OpenCategory(WordsDatabase.WordCategory category)
    {
        Reset();

        editingCategory = category.Name;
        categoryTitle.text = category.Name;

        foreach (string word in category.Words)
            AddWord(word);

        addWordBox.transform.SetAsLastSibling();
    }

    public void AddBox()
    {
        AddWord(string.Empty);
        addWordBox.transform.SetAsLastSibling();
    }

    public void DeleteBox(WordBox wordBox)
    {
        wordBoxes.Remove(wordBox);
        Destroy(wordBox.gameObject);

        addWordBox.transform.SetAsLastSibling();
    }

    public void ImportWords()
    {
        string[] splitWords = bulkInputField.text.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string splitWord in splitWords)
        {
            string word = splitWord.Trim();

            if (!string.IsNullOrEmpty(word))
                AddWord(word);
        }

        addWordBox.transform.SetAsLastSibling();
        bulkInputField.text = string.Empty;
    }

    public void Validate()
    {
        string title = categoryTitle.text.Trim();

        if (string.IsNullOrEmpty(title))
        {
            ShowError(EmptyTitleMessage);
            return;
        }

        List<string> words = new();

        foreach (WordBox wordBox in wordBoxes)
        {
            string word = wordBox.GetWord().Trim();

            if (string.IsNullOrEmpty(word))
                continue;

            if (!words.Contains(word))
                words.Add(word);
        }

        if (words.Count == 0)
        {
            ShowError(EmptyWordsMessage);
            return;
        }

        if (editingCategory == null)
        {
            CreateCategory(title, words);
        }
        else
        {
            CustomCategoryManager.SaveCategory(
                editingCategory,
                title,
                words
            );
        }

        menuManager.ToggleMenu(categoryHandler);
        categoryHandler.DisplayCategories();
    }

    private void AddWord(string word)
    {
        WordBox wordBox = Instantiate(
            wordBoxPrefab,
            contentParent
        );

        wordBoxes.Add(wordBox);
        wordBox.SetWord(word);
        wordBox.SetHandler(this);
    }

    private void CreateCategory(string title, List<string> words)
    {
        CustomCategoryManager.CreateCategory(
            title,
            words
        );
    }

    private void ShowError(string message)
    {
        ErrorPopup popup = Instantiate(errorPopupPrefab);
        popup.Setup(message);
    }
}
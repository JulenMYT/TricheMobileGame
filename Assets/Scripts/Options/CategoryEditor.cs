using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryEditor : MenuPanel
{
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

    public void ResetEditor()
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
        ResetEditor();

        editingCategory = category.Name;
        categoryTitle.text = category.Name;

        foreach (string word in category.Words)
            AddWord(word);

        addWordBox.transform.SetAsLastSibling();
    }

    public void AddBox()
    {
        WordBox wordBox = AddWord(string.Empty);
        addWordBox.transform.SetAsLastSibling();

        if (contentParent is RectTransform content)
            UIUtility.ScrollToBottom(content);

        wordBox.Focus();
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

        string nameError = CustomCategoryManager.ValidateName(title, editingCategory);

        if (nameError != null)
        {
            ShowError(nameError);
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

        string saveError = CustomCategoryManager.SaveCategory(
            editingCategory,
            title,
            words
        );

        // En cas d'échec, l'éditeur reste ouvert pour ne pas perdre les mots saisis.
        if (saveError != null)
        {
            ShowError(saveError);
            return;
        }

        menuManager.ToggleMenu(categoryHandler);
        categoryHandler.DisplayCategories();
    }

    private WordBox AddWord(string word)
    {
        WordBox wordBox = Instantiate(
            wordBoxPrefab,
            contentParent
        );

        wordBoxes.Add(wordBox);
        wordBox.SetWord(word);
        wordBox.SetHandler(this);

        return wordBox;
    }

    public void ShowError(string message)
    {
        ErrorPopup popup = Instantiate(errorPopupPrefab);
        popup.Setup(message);
    }
}
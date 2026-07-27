using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Transform contentParent;

    [SerializeField] private Button validateButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button importButton;

    [SerializeField] private WordBox wordBoxPrefab;

    private List<WordBox> wordBoxes = new();

    [SerializeField] private TMP_InputField categoryTitle;
    [SerializeField] private TMP_InputField bulkInputField;

    [SerializeField] private AddBox addCategoryBoxPrefab;
    private AddBox addCategoryBox;

    private string editingCategory;

    public event Action OnValidate;
    public event Action OnCancel;

    private void Awake()
    {
        cancelButton.onClick.AddListener(()=>OnCancel?.Invoke());
        validateButton.onClick.AddListener(Validate);
        importButton.onClick.AddListener(ImportWords);
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

    public void Reset()
    {
        editingCategory = null;

        foreach (WordBox wordBox in wordBoxes)
            wordBox.OnDelete -= OnBoxDelete;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        wordBoxes.Clear();

        categoryTitle.text = string.Empty;
        bulkInputField.text = string.Empty;

        addCategoryBox = Instantiate(
            addCategoryBoxPrefab,
            contentParent
        );

        addCategoryBox.OnClick += OnAddClicked;
    }

    public void OpenCategory(WordsDatabase.WordCategory category)
    {
        Reset();

        editingCategory = category.categoryName;

        categoryTitle.text = category.categoryName;

        foreach (string word in category.words)
        {
            WordBox wordBox = Instantiate(wordBoxPrefab, contentParent);

            wordBoxes.Add(wordBox);

            wordBox.OnDelete += OnBoxDelete;

            wordBox.SetWord(word);
        }

        addCategoryBox.transform.SetAsLastSibling();
    }

    private void OnAddClicked()
    {
        WordBox wordBox = Instantiate(wordBoxPrefab, contentParent);
        wordBoxes.Add(wordBox);
        wordBox.OnDelete += OnBoxDelete;
        addCategoryBox.transform.SetAsLastSibling();
    }

    private void OnBoxDelete(WordBox wordBox)
    {
        wordBoxes.Remove(wordBox);
        wordBox.OnDelete-= OnBoxDelete;
    }

    private void ImportWords()
    {
        string text = bulkInputField.text;

        string[] splitWords = text.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string splitWord in splitWords)
        {
            string word = splitWord.Trim();

            if (string.IsNullOrEmpty(word))
                continue;

            WordBox wordBox = Instantiate(
                wordBoxPrefab,
                contentParent
            );

            wordBoxes.Add(wordBox);

            wordBox.OnDelete += OnBoxDelete;

            wordBox.SetWord(word);
        }

        addCategoryBox.transform.SetAsLastSibling();

        bulkInputField.text = string.Empty;
    }

    private void Validate()
    {
        List<string> words = new List<string>();

        if (string.IsNullOrWhiteSpace(categoryTitle.text))
            return;

        foreach (WordBox wordBox in wordBoxes)
        {
            string newWord = wordBox.GetWord().Trim();

            if (string.IsNullOrEmpty(newWord))
                continue;

            if (!words.Contains(newWord))
                words.Add(newWord);
        }

        if (words.Count == 0)
            return;

        if (editingCategory == null)
        {
            CreateCategory(
                categoryTitle.text,
                words
            );
        }
        else
        {
            CustomCategoryManager.SaveCategory(
                editingCategory,
                categoryTitle.text,
                words
            );
        }

        OnValidate?.Invoke();
    }

    private void CreateCategory(string title, List<string> words)
    {
        CustomCategoryManager.CreateCategory(
            title,
            words
        );
    }
}

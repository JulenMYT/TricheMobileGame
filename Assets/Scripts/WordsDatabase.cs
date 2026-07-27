using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WordsDatabase
{
    private static bool initialized = false;

    private static List<WordCategory> categories;
    private static Dictionary<string, WordCategory> categoriesDict;

    private const string PATH = "Categories";

    private static List<WordCategory> shuffledCategories = new List<WordCategory>();
    private static int shuffledIndex = 0;

    private static Dictionary<string, List<WordCategory>> allowedShuffles = new Dictionary<string, List<WordCategory>>();
    private static Dictionary<string, int> allowedIndices = new Dictionary<string, int>();

    public static void Initialize()
    {
        if (initialized)
            return;

        categories = new List<WordCategory>();
        categoriesDict = new Dictionary<string, WordCategory>();

        LoadBaseCategories();
        LoadCustomCategories();

        initialized = true;
    }

    private static void LoadBaseCategories()
    {
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>(PATH);

        foreach (TextAsset textAsset in textAssets)
        {
            List<string> words = ParseWords(textAsset.text);

            if (words.Count == 0)
                continue;

            WordCategory category = new WordCategory(
                textAsset.name,
                words,
                false
            );

            AddCategory(category);
        }
    }

    private static void LoadCustomCategories()
    {
        string categoriesPath = Path.Combine(
            Application.persistentDataPath,
            PATH
        );

        if (!Directory.Exists(categoriesPath))
            return;

        string[] files = Directory.GetFiles(
            categoriesPath,
            "*.txt"
        );

        foreach (string file in files)
        {
            string text = File.ReadAllText(file);

            List<string> words = ParseWords(text);

            if (words.Count == 0)
                continue;

            string categoryName = Path.GetFileNameWithoutExtension(file);

            WordCategory category = new WordCategory(
                categoryName,
                words,
                true
            );

            AddCategory(category);
        }
    }

    private static List<string> ParseWords(string text)
    {
        string[] lines = text.Split(
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        List<string> words = new List<string>();

        foreach (string line in lines)
        {
            string word = line.Trim();

            if (!string.IsNullOrEmpty(word))
                words.Add(word);
        }

        return words;
    }

    private static void AddCategory(WordCategory category)
    {
        if (categoriesDict.ContainsKey(category.categoryName))
            return;

        categories.Add(category);
        categoriesDict.Add(
            category.categoryName,
            category
        );
    }

    private static void ShuffleList(List<WordCategory> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);

            WordCategory tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    private static WordCategory GetNextShuffled()
    {
        if (shuffledCategories.Count != categories.Count)
        {
            shuffledCategories = new List<WordCategory>(categories);
            ShuffleList(shuffledCategories);
            shuffledIndex = 0;
        }

        if (shuffledIndex >= shuffledCategories.Count)
        {
            ShuffleList(shuffledCategories);
            shuffledIndex = 0;
        }

        return shuffledCategories[shuffledIndex++];
    }

    private static WordCategory GetNextShuffled(
        List<WordCategory> list,
        ref int index
    )
    {
        if (list.Count == 0)
            return null;

        if (index >= list.Count)
        {
            ShuffleList(list);
            index = 0;
        }

        return list[index++];
    }

    public static (string category, string word) GetRandomWord(
        bool shuffle = true
    )
    {
        if (!initialized)
            Initialize();

        if (categories == null || categories.Count == 0)
            return ("None", "???");

        if (shuffle)
        {
            WordCategory category = GetNextShuffled();

            return (
                category.categoryName,
                category.GetRandomWord()
            );
        }

        WordCategory randomCategory =
            categories[Random.Range(0, categories.Count)];

        return (
            randomCategory.categoryName,
            randomCategory.GetRandomWord()
        );
    }

    public static (string category, string word) GetRandomWord(
        List<string> allowedCategories,
        bool shuffle = true
    )
    {
        if (!initialized)
            Initialize();

        if (allowedCategories == null || allowedCategories.Count == 0)
            return GetRandomWord(shuffle);

        if (!shuffle)
        {
            List<WordCategory> valid = new List<WordCategory>();

            foreach (string name in allowedCategories)
            {
                if (categoriesDict.TryGetValue(
                    name,
                    out WordCategory category
                ))
                {
                    valid.Add(category);
                }
            }

            if (valid.Count == 0)
                return GetRandomWord(shuffle);

            WordCategory selected =
                valid[Random.Range(0, valid.Count)];

            return (
                selected.categoryName,
                selected.GetRandomWord()
            );
        }

        string key = string.Join("|", allowedCategories);

        if (!allowedShuffles.ContainsKey(key))
        {
            List<WordCategory> valid = new List<WordCategory>();

            foreach (string name in allowedCategories)
            {
                if (categoriesDict.TryGetValue(
                    name,
                    out WordCategory category
                ))
                {
                    valid.Add(category);
                }
            }

            if (valid.Count == 0)
                return GetRandomWord(shuffle);

            ShuffleList(valid);

            allowedShuffles[key] = valid;
            allowedIndices[key] = 0;
        }

        List<WordCategory> list = allowedShuffles[key];

        int index = allowedIndices[key];

        WordCategory categoryShuffled =
            GetNextShuffled(list, ref index);

        allowedIndices[key] = index;

        return (
            categoryShuffled.categoryName,
            categoryShuffled.GetRandomWord()
        );
    }

    public static List<WordCategory> GetAllCategories()
    {
        if (!initialized)
            Initialize();

        return categories;
    }

    public static void Reload()
    {
        initialized = false;

        categories = null;
        categoriesDict = null;

        shuffledCategories = new List<WordCategory>();
        shuffledIndex = 0;

        allowedShuffles.Clear();
        allowedIndices.Clear();

        Initialize();
    }

    public static WordCategory GetWordCategory(string name)
    {
        foreach (WordCategory category in GetAllCategories())
        {
            if (category.categoryName.Equals(name))
            {
                return category;
            }
        }
        return null;
    }

    public class WordCategory
    {
        public string categoryName;
        public List<string> words;
        public bool IsCustom { get; }

        public WordCategory(
            string categoryName,
            List<string> words,
            bool isCustom
        )
        {
            this.categoryName = categoryName;
            this.words = words;
            IsCustom = isCustom;
        }

        public string GetRandomWord()
        {
            return words[Random.Range(0, words.Count)];
        }

        public int GetWordCount()
        {
            return words.Count;
        }
    }
}
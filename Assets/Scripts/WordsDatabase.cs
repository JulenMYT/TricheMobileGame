using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WordsDatabase
{
    private const string PATH = "Categories";

    private static bool initialized;

    private static readonly List<WordCategory> categories = new();
    private static readonly Dictionary<string, WordCategory> categoriesByName = new();

    private static List<WordCategory> shuffledCategories = new();
    private static int shuffledIndex;

    public static void Initialize()
    {
        if (initialized)
            return;

        categories.Clear();
        categoriesByName.Clear();

        LoadBaseCategories();
        LoadCustomCategories();

        ResetShuffle();

        initialized = true;
    }

    private static void LoadBaseCategories()
    {
        TextAsset[] assets = Resources.LoadAll<TextAsset>(PATH);

        foreach (TextAsset asset in assets)
        {
            List<string> words = ParseWords(asset.text);

            if (words.Count == 0)
                continue;

            AddCategory(new WordCategory(
                asset.name,
                words,
                false
            ));
        }
    }

    private static void LoadCustomCategories()
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            PATH
        );

        if (!Directory.Exists(path))
            return;

        foreach (string file in Directory.GetFiles(path, "*.txt"))
        {
            List<string> words = ParseWords(File.ReadAllText(file));

            if (words.Count == 0)
                continue;

            AddCategory(new WordCategory(
                Path.GetFileNameWithoutExtension(file),
                words,
                true
            ));
        }
    }

    private static List<string> ParseWords(string text)
    {
        List<string> words = new();

        foreach (string line in text.Split(
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries))
        {
            string word = line.Trim();

            if (!string.IsNullOrEmpty(word))
                words.Add(word);
        }

        return words;
    }

    private static void AddCategory(WordCategory category)
    {
        if (categoriesByName.ContainsKey(category.Name))
            return;

        categories.Add(category);
        categoriesByName.Add(category.Name, category);
    }

    private static void ResetShuffle()
    {
        shuffledCategories = new List<WordCategory>(categories);

        Shuffle(shuffledCategories);

        shuffledIndex = 0;
    }

    private static void Shuffle(List<WordCategory> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int index = Random.Range(i, list.Count);

            (list[i], list[index]) = (list[index], list[i]);
        }
    }

    private static WordCategory GetNextCategory()
    {
        if (shuffledCategories.Count == 0)
            return null;

        if (shuffledIndex >= shuffledCategories.Count)
            ResetShuffle();

        return shuffledCategories[shuffledIndex++];
    }

    public static (string category, string word) GetRandomWord(bool shuffle = true)
    {
        if (!initialized)
            Initialize();

        if (categories.Count == 0)
            return ("None", "???");

        WordCategory category = shuffle ? GetNextCategory() : categories[Random.Range(0, categories.Count)];

        return (category.Name, category.GetRandomWord());
    }

    public static (string category, string word) GetRandomWord(List<string> allowedCategories, bool shuffle = true)
    {
        if (!initialized)
            Initialize();

        if (allowedCategories == null || allowedCategories.Count == 0)
            return GetRandomWord(shuffle);

        List<WordCategory> validCategories = new();

        foreach (string name in allowedCategories)
        {
            if (categoriesByName.TryGetValue(name, out WordCategory category))
                validCategories.Add(category);
        }

        if (validCategories.Count == 0)
            return GetRandomWord(shuffle);

        WordCategory selected;

        if (shuffle)
        {
            selected = GetShuffledAllowedCategory(allowedCategories, validCategories);
        }
        else
        {
            selected = validCategories[
                Random.Range(0, validCategories.Count)
            ];
        }

        return (selected.Name, selected.GetRandomWord());
    }

    private static readonly Dictionary<string, List<WordCategory>> shuffledAllowedCategories = new();
    private static readonly Dictionary<string, int> shuffledAllowedIndices = new();

    private static WordCategory GetShuffledAllowedCategory(List<string> allowedNames, List<WordCategory> validCategories)
    {
        string key = string.Join("|", allowedNames);

        if (!shuffledAllowedCategories.TryGetValue(key, out List<WordCategory> shuffled))
        {
            shuffled = new List<WordCategory>(validCategories);
            Shuffle(shuffled);

            shuffledAllowedCategories[key] = shuffled;
            shuffledAllowedIndices[key] = 0;
        }

        int index = shuffledAllowedIndices[key];

        if (index >= shuffled.Count)
        {
            Shuffle(shuffled);
            index = 0;
        }

        shuffledAllowedIndices[key] = index + 1;

        return shuffled[index];
    }

    public static List<WordCategory> GetAllCategories()
    {
        if (!initialized)
            Initialize();

        return categories;
    }

    public static WordCategory GetWordCategory(string name)
    {
        if (!initialized)
            Initialize();

        categoriesByName.TryGetValue(name, out WordCategory category);

        return category;
    }

    public static void Reload()
    {
        initialized = false;

        categories.Clear();
        categoriesByName.Clear();

        shuffledCategories.Clear();
        shuffledAllowedCategories.Clear();
        shuffledAllowedIndices.Clear();

        Initialize();
    }

    public class WordCategory
    {
        public string Name { get; }
        public List<string> Words { get; }
        public bool IsCustom { get; }

        public WordCategory(
            string name,
            List<string> words,
            bool isCustom)
        {
            Name = name;
            Words = words;
            IsCustom = isCustom;
        }

        public string GetRandomWord()
        {
            return Words[Random.Range(0, Words.Count)];
        }

        public int GetWordCount()
        {
            return Words.Count;
        }
    }
}
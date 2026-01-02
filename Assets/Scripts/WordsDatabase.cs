using System.Collections.Generic;
using UnityEngine;

public static class WordsDatabase
{
    private static bool initialized = false;
    private static List<CategorySO> categories;
    private static Dictionary<string, CategorySO> categoriesDict;
    private const string PATH = "Categories";

    private static List<CategorySO> shuffledCategories = new List<CategorySO>();
    private static int shuffledIndex = 0;

    private static Dictionary<string, List<CategorySO>> allowedShuffles = new Dictionary<string, List<CategorySO>>();
    private static Dictionary<string, int> allowedIndices = new Dictionary<string, int>();

    public static void Initialize()
    {
        if (categories != null)
            return;

        categories = new List<CategorySO>(Resources.LoadAll<CategorySO>(PATH));
        categoriesDict = new Dictionary<string, CategorySO>(categories.Count);

        foreach (var category in categories)
        {
            if (!categoriesDict.ContainsKey(category.categoryName))
                categoriesDict.Add(category.categoryName, category);
        }

        initialized = true;
    }

    private static void ShuffleList(List<CategorySO> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    private static CategorySO GetNextShuffled()
    {
        if (shuffledCategories.Count != categories.Count)
        {
            shuffledCategories = new List<CategorySO>(categories);
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

    private static CategorySO GetNextShuffled(List<CategorySO> list, ref int index)
    {
        if (list.Count == 0) return null;

        if (index >= list.Count)
        {
            ShuffleList(list);
            index = 0;
        }

        return list[index++];
    }

    public static (string category, string word) GetRandomWord(bool shuffle = true)
    {
        if (!initialized)
            Initialize();

        if (categories == null || categories.Count == 0)
            return ("None", "???");

        if (shuffle)
        {
            var category = GetNextShuffled();
            return (category.categoryName, category.GetRandomWord());
        }
        else
        {
            var category = categories[Random.Range(0, categories.Count)];
            return (category.categoryName, category.GetRandomWord());
        }
    }

    public static (string category, string word) GetRandomWord(List<string> allowedCategories, bool shuffle = true)
    {
        if (!initialized)
            Initialize();

        if (allowedCategories == null || allowedCategories.Count == 0)
            return GetRandomWord(shuffle);

        if (!shuffle)
        {
            List<CategorySO> valid = new List<CategorySO>();
            foreach (var name in allowedCategories)
            {
                if (categoriesDict.TryGetValue(name, out var cat))
                    valid.Add(cat);
            }

            if (valid.Count == 0)
                return GetRandomWord(shuffle);

            var selected = valid[Random.Range(0, valid.Count)];
            return (selected.categoryName, selected.GetRandomWord());
        }

        string key = string.Join("|", allowedCategories);
        if (!allowedShuffles.ContainsKey(key))
        {
            List<CategorySO> valid = new List<CategorySO>();
            foreach (var name in allowedCategories)
            {
                if (categoriesDict.TryGetValue(name, out var cat))
                    valid.Add(cat);
            }

            if (valid.Count == 0)
                return GetRandomWord(shuffle);

            ShuffleList(valid);
            allowedShuffles[key] = valid;
            allowedIndices[key] = 0;
        }

        var list = allowedShuffles[key];
        int index = allowedIndices[key];
        var categoryShuffled = GetNextShuffled(list, ref index);
        allowedIndices[key] = index;

        return (categoryShuffled.categoryName, categoryShuffled.GetRandomWord());
    }

    public static List<CategorySO> GetAllCategories()
    {
        if (!initialized)
            Initialize();

        return categories;
    }
}

using System.Collections.Generic;
using UnityEngine;

public static class CategorySettings
{
    private const string CATEGORY_KEY_PREFIX = "Category_";

    private static HashSet<string> enabledCategories = new HashSet<string>();

    private static bool initialized = false;

    public static void Initialize()
    {
        enabledCategories.Clear();

        var allCategories = WordsDatabase.GetAllCategories();

        foreach (var category in allCategories)
        {
            string key = CATEGORY_KEY_PREFIX + category.Name;

            bool enabled = PlayerPrefs.GetInt(key, 1) == 1;

            if (enabled)
                enabledCategories.Add(category.Name);
        }

        initialized = true;
    }

    public static void SetCategoryEnabled(string categoryName, bool enabled)
    {
        if (!initialized)
            Initialize();

        if (enabled)
            enabledCategories.Add(categoryName);
        else
            enabledCategories.Remove(categoryName);

        PlayerPrefs.SetInt(
            CATEGORY_KEY_PREFIX + categoryName,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public static bool IsCategoryEnabled(string categoryName)
    {
        if (!initialized)
            Initialize();

        return enabledCategories.Contains(categoryName);
    }

    public static List<string> GetEnabledCategoryNames()
    {
        if (!initialized)
            Initialize();

        return new List<string>(enabledCategories);
    }

    public static int GetEnabledCount()
    {
        if (!initialized)
            Initialize();

        return enabledCategories.Count;
    }
}
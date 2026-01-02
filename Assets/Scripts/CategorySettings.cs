using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CategorySettings
{
    private static HashSet<string> enabledCategories = new HashSet<string>();

    private static bool initialized = false;

    public static void Initialize()
    {
        enabledCategories.Clear();

        var allCategories = WordsDatabase.GetAllCategories();
        foreach (var category in allCategories)
            enabledCategories.Add(category.categoryName);

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

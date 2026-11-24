using System.Collections.Generic;
using UnityEngine;

// L'état activé/désactivé de chaque catégorie est lu directement dans les PlayerPrefs
// (activée par défaut), pour que les catégories créées ou renommées en cours de
// partie soient prises en compte sans cache à resynchroniser.
public static class CategorySettings
{
    private const string CATEGORY_KEY_PREFIX = "Category_";

    private static string GetKey(string categoryName) => CATEGORY_KEY_PREFIX + categoryName;

    public static void SetCategoryEnabled(string categoryName, bool enabled)
    {
        PlayerPrefs.SetInt(GetKey(categoryName), enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool IsCategoryEnabled(string categoryName)
    {
        return PlayerPrefs.GetInt(GetKey(categoryName), 1) == 1;
    }

    public static List<string> GetEnabledCategoryNames()
    {
        List<string> names = new();

        foreach (WordsDatabase.WordCategory category in WordsDatabase.GetAllCategories())
        {
            if (IsCategoryEnabled(category.Name))
                names.Add(category.Name);
        }

        return names;
    }

    public static int GetEnabledCount()
    {
        return GetEnabledCategoryNames().Count;
    }

    // Conserve l'état activé/désactivé quand une catégorie est renommée.
    public static void RenameCategory(string oldName, string newName)
    {
        if (oldName == newName)
            return;

        bool enabled = IsCategoryEnabled(oldName);

        PlayerPrefs.DeleteKey(GetKey(oldName));
        SetCategoryEnabled(newName, enabled);
    }

    public static void ForgetCategory(string categoryName)
    {
        PlayerPrefs.DeleteKey(GetKey(categoryName));
        PlayerPrefs.Save();
    }
}

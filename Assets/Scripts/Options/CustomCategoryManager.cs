using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CustomCategoryManager
{
    private const string FOLDER = "Categories";

    private static string FolderPath =>
        Path.Combine(
            Application.persistentDataPath,
            FOLDER
        );

    public static void CreateCategory(
        string categoryName,
        List<string> words
    )
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return;

        if (CategoryExists(categoryName))
            return;

        EnsureFolder();

        SaveCategory(
            null,
            categoryName,
            words
        );
    }

    public static void SaveCategory(
        string oldCategoryName,
        string newCategoryName,
        List<string> words
    )
    {
        if (string.IsNullOrWhiteSpace(newCategoryName))
            return;

        if (oldCategoryName != newCategoryName &&
            CategoryExists(newCategoryName))
            return;

        EnsureFolder();

        List<string> cleanWords = new List<string>();

        foreach (string word in words)
        {
            string cleanWord = word.Trim();

            if (!string.IsNullOrEmpty(cleanWord) &&
                !cleanWords.Contains(cleanWord))
            {
                cleanWords.Add(cleanWord);
            }
        }

        string newPath = GetCategoryPath(newCategoryName);

        if (!string.IsNullOrEmpty(oldCategoryName))
        {
            string oldPath = GetCategoryPath(oldCategoryName);

            if (oldPath != newPath && File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }

        File.WriteAllLines(
            newPath,
            cleanWords
        );

        WordsDatabase.Reload();
    }

    public static void DeleteCategory(
        string categoryName
    )
    {
        string path = GetCategoryPath(categoryName);

        if (!File.Exists(path))
            return;

        File.Delete(path);

        WordsDatabase.Reload();
    }

    public static List<string> GetWords(
        string categoryName
    )
    {
        string path = GetCategoryPath(categoryName);

        if (!File.Exists(path))
            return new List<string>();

        return new List<string>(
            File.ReadAllLines(path)
        );
    }

    public static bool CategoryExists(
        string categoryName
    )
    {
        return File.Exists(
            GetCategoryPath(categoryName)
        );
    }

    public static List<string> GetCategoryNames()
    {
        EnsureFolder();

        string[] files = Directory.GetFiles(
            FolderPath,
            "*.txt"
        );

        List<string> categories = new List<string>();

        foreach (string file in files)
        {
            categories.Add(
                Path.GetFileNameWithoutExtension(file)
            );
        }

        return categories;
    }

    private static string GetCategoryPath(
        string categoryName
    )
    {
        return Path.Combine(
            FolderPath,
            categoryName + ".txt"
        );
    }

    private static void EnsureFolder()
    {
        if (!Directory.Exists(FolderPath))
            Directory.CreateDirectory(FolderPath);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CustomCategoryManager
{
    private const string FOLDER = "Categories";

    public const string EmptyNameMessage = "La catégorie doit avoir un nom !";
    public const string DuplicateNameMessage = "Une catégorie porte déjà ce nom !";
    public const string InvalidNameMessage = "Le nom ne peut pas contenir / \\ : * ? \" < > |";
    public const string SaveFailedMessage = "Impossible d'enregistrer la catégorie !";

    // Caractères interdits dans un nom de fichier sur au moins une plateforme cible
    // (Windows, Android, iOS, WebGL), en plus de ceux du système courant.
    private static readonly char[] ForbiddenChars = { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

    private static string FolderPath =>
        Path.Combine(
            Application.persistentDataPath,
            FOLDER
        );

    // Renvoie un message d'erreur si le nom est invalide, sinon null.
    // 'currentName' est le nom actuel de la catégorie modifiée (null pour une création).
    public static string ValidateName(string categoryName, string currentName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return EmptyNameMessage;

        if (categoryName.IndexOfAny(ForbiddenChars) >= 0 ||
            categoryName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            categoryName.EndsWith("."))
            return InvalidNameMessage;

        // Comparaison insensible à la casse : sur Windows, "animaux.txt" et
        // "Animaux.txt" sont le même fichier.
        foreach (WordsDatabase.WordCategory category in WordsDatabase.GetAllCategories())
        {
            bool sameName = string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase);
            bool isItself = currentName != null &&
                string.Equals(category.Name, currentName, StringComparison.OrdinalIgnoreCase);

            if (sameName && !isItself)
                return DuplicateNameMessage;
        }

        return null;
    }

    // Crée ou met à jour une catégorie. Renvoie null en cas de succès, sinon un message d'erreur.
    public static string SaveCategory(
        string oldCategoryName,
        string newCategoryName,
        List<string> words
    )
    {
        string error = ValidateName(newCategoryName, oldCategoryName);

        if (error != null)
            return error;

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

        try
        {
            EnsureFolder();

            string newPath = GetCategoryPath(newCategoryName);

            // On écrit le nouveau fichier avant de supprimer l'ancien,
            // pour ne rien perdre si l'écriture échoue.
            File.WriteAllLines(newPath, cleanWords);

            if (!string.IsNullOrEmpty(oldCategoryName))
            {
                string oldPath = GetCategoryPath(oldCategoryName);

                // Sur Windows, un simple changement de casse désigne le même fichier :
                // il ne faut surtout pas le supprimer.
                if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }

                CategorySettings.RenameCategory(oldCategoryName, newCategoryName);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return SaveFailedMessage;
        }

        PersistentFiles.Flush();
        WordsDatabase.Reload();

        return null;
    }

    public static void DeleteCategory(
        string categoryName
    )
    {
        string path = GetCategoryPath(categoryName);

        if (!File.Exists(path))
            return;

        File.Delete(path);
        PersistentFiles.Flush();

        CategorySettings.ForgetCategory(categoryName);
        WordsDatabase.Reload();
    }

    public static bool CategoryExists(
        string categoryName
    )
    {
        return File.Exists(
            GetCategoryPath(categoryName)
        );
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

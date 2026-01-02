using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CategoryImporter
{
    // Menu Unity pour lancer l'import
    [MenuItem("Tools/Import Categories")]
    public static void ImportCategories()
    {
        string folderPath = "Assets/CategoryData"; // dossier où sont tes fichiers texte
        string outputFolder = "Assets/Resources/Categories"; // dossier où générer les ScriptableObjects

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string[] files = Directory.GetFiles(folderPath, "*.txt");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string[] lines = File.ReadAllLines(file);

            CategorySO category = ScriptableObject.CreateInstance<CategorySO>();
            category.categoryName = fileName;
            category.words = new List<string>();

            foreach (string line in lines)
            {
                string word = line.Trim();
                if (!string.IsNullOrEmpty(word))
                    category.words.Add(word);
            }

            string assetPath = Path.Combine(outputFolder, fileName + ".asset");
            AssetDatabase.CreateAsset(category, assetPath);
            Debug.Log("Created CategorySO: " + fileName + " with " + category.words.Count + " words");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

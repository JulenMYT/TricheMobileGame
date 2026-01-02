using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCategory", menuName = "Words/Category")]
public class CategorySO : ScriptableObject
{
    public string categoryName;
    public List<string> words;

    public string GetRandomWord()
    {
        if (words == null || words.Count == 0)
            return "???";

        int index = Random.Range(0, words.Count);
        return words[index];
    }

    public int GetWordCount()
    {
        return words.Count;
    }
}

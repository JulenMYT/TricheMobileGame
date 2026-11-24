using TMPro;
using UnityEngine;

public class WordBox : MonoBehaviour
{
    [SerializeField] private TMP_InputField wordField;

    private CategoryEditor categoryEditor;

    public void SetHandler(CategoryEditor handler)
    {
        categoryEditor = handler;
    }

    public void Delete()
    {
        categoryEditor.DeleteBox(this);
    }

    public string GetWord()
    {
        return wordField.text;
    }

    public void Focus()
    {
        UIUtility.Focus(wordField);
    }

    public void SetWord(string word)
    {
        wordField.text = word;
    }
}
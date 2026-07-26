using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordBox : MonoBehaviour
{
    [SerializeField] private TMP_InputField wordField;
    [SerializeField] private Button deleteButton;

    public event Action<WordBox> OnDelete;

    private void Start()
    {
        deleteButton.onClick.AddListener(Delete);
    }

    private void Delete()
    {
        OnDelete?.Invoke(this);
        Destroy(gameObject);
    }

    public string GetWord()
    {
        return wordField.text;
    }

    public void SetWord(string word)
    {
        wordField.text = word;
    }
}

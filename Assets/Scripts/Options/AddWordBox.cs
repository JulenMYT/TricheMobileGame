using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddWordBox : MonoBehaviour
{
    private CategoryEditor categoryEditor;

    public void Setup(CategoryEditor categoryEditor)
    {
        this.categoryEditor = categoryEditor;
    }

    public void Add()
    {
        categoryEditor.AddBox();
    }
}

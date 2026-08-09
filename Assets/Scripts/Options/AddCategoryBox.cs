using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCategoryBox : MonoBehaviour
{
    private CategoryHandler categoryHandler;

    public void Setup(CategoryHandler categoryHandler)
    {
        this.categoryHandler = categoryHandler;
    }

    public void Add()
    {
        categoryHandler.AddCategory();
    }
}

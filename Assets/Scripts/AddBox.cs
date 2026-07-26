using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddBox : MonoBehaviour
{
    [SerializeField] private Button button;

    public event Action OnClick;

    private void Awake()
    {
        button.onClick.AddListener(() => OnClick?.Invoke());
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public abstract class Widget : MonoBehaviour
{
    [Header("Widget Settings")]
    public bool autoSubscribe = true;

    protected virtual void Start()
    {
        if(autoSubscribe)
            Subscribe();

    }

    protected virtual void OnDestroy()
    {
        UnSubscribe();
    }

    protected abstract void Subscribe();
    protected abstract void UnSubscribe();
}

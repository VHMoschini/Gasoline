using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyBinder : MonoBehaviour
{
    [SerializeField] private List<KeyToBind> bindings = new List<KeyToBind>();

    void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach(KeyToBind bind in bindings)
            {
                if (Input.GetKeyDown(bind.key))
                {
                    bind.callback.Invoke();
                }
            }
        }
    }
}

[Serializable]
public struct KeyToBind
{
    [SerializeField] internal KeyCode key;
    [SerializeField] internal UnityEvent callback;
}


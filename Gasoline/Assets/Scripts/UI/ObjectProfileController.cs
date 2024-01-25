using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProfileController : MonoBehaviour
{
    [SerializeField] private List<ObjectProfile> objects = new List<ObjectProfile>();

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

[Serializable]
public struct ObjectProfile
{
    [SerializeField] internal string profileName;
    [SerializeField] internal List<GameObject> objs;
}

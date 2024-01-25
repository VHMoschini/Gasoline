using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProfileController : MonoBehaviour
{
    [SerializeField] private List<ObjectProfile> objects = new List<ObjectProfile>();

    private int actualIndex = 0;

    void Start()
    {
        RefreshObjsProfile();
    }

    private void RefreshObjsProfile()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            bool activeState = false;

            if (actualIndex == i)
            {
                activeState = true;
            }

            foreach (GameObject obj in objects[i].objs)
            {
                obj.SetActive(activeState);
            }
        }
    }

    public void IncrementObjProfile()
    {
        actualIndex++;

        if (actualIndex == objects.Count)
        {
            actualIndex = 0;
        }

        RefreshObjsProfile();
    }
    public void DecrementObjProfile()
    {
        actualIndex--;

        if (actualIndex < 0)
        {
            actualIndex = objects.Count - 1;
        }

        RefreshObjsProfile();
    }
}

[Serializable]
public struct ObjectProfile
{
    [SerializeField] internal string profileName;
    [SerializeField] internal List<GameObject> objs;
}

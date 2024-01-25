using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProfileController : MonoBehaviour
{
    [SerializeField] private List<ObjectProfile> objects = new List<ObjectProfile>();
    [SerializeField] private bool canLoop = true;

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

        if (actualIndex + 1 == objects.Count)
        {
            if (canLoop)
                actualIndex = 0;
        }
        else
        {
            actualIndex++;
        }

        RefreshObjsProfile();
    }
    public void DecrementObjProfile()
    {
        if (actualIndex - 1 < 0)
        {
            if (canLoop)
                actualIndex = objects.Count - 1;
        }
        else
        {
            actualIndex--;
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

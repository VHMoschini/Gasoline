using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolInstantiator : MonoBehaviour
{

    private List<GameObject> objects = new List<GameObject>();

    internal void instantiate(GameObject obj)
    {
        foreach (GameObject o in objects)
        {
            Destroy(o);
        }

        objects.Add(Instantiate(obj, transform));
    }
}

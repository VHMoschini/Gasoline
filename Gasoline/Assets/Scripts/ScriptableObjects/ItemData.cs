using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SaveItem;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Space(10)]
    [SerializeField] internal ItemType type;

    [Space(10)]
    [SerializeField] internal Color color;

    [Space(10)]
    [SerializeField] internal bool isColor;
    [SerializeField] internal bool isIcon;
    [SerializeField] internal Material colorMaterial;

    [Space(10)]
    [SerializeField] internal GameObject showcaseObj;
    [SerializeField] internal GameObject carObject;

}

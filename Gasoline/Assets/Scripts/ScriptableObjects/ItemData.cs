using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Space(10)]
    [SerializeField] internal string itemName;
    [SerializeField] internal float itemPrice;

    [Space(10)]
    [SerializeField] internal Sprite itemBaseImage;
    [SerializeField] internal Sprite itemAuxiliarImage;

    [Space(10)]
    [SerializeField] internal AnimatorOverrideController anim;

    [Space(10)]
    [SerializeField] internal Color color;

}

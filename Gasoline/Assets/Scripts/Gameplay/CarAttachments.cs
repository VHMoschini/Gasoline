using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAttachments : MonoBehaviour
{

    [SerializeField] private PoolInstantiator Bumper_Instantiator;
    [SerializeField] private PoolInstantiator Spoiler_Instantiator;
    [SerializeField] private PoolInstantiator Wheels_Insantiators;

    [SerializeField] private MeshRenderer Color_Insantiator;


    void OnEnable()
    {
        if (PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Bumper) != null)
        {
            Bumper_Instantiator.instantiate(PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Bumper).carObject);
        }

        if (PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Spoiler) != null)
        {
            Spoiler_Instantiator.instantiate(PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Spoiler).carObject);
        }

        if (PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Wheel) != null)
        {
            Wheels_Insantiators.instantiate(PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Wheel).carObject);
        }



        if (PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Color) != null)
        {
            Color_Insantiator.material = PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Color).colorMaterial;
        }
    }



}

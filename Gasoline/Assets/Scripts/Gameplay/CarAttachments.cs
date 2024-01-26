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
        Bumper_Instantiator.instantiate(PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Bumper).carObject);
        Spoiler_Instantiator.instantiate(PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Spoiler).carObject);
        Wheels_Insantiators.instantiate(PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Wheel).carObject);

        Color_Insantiator.material = PlayerDatabase.GetEquipedItemsByType(SaveItem.ItemType.Color).colorMaterial;
    }


}

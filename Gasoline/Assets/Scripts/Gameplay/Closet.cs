using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Closet : MonoBehaviour
{
    #region [ VARIABLES ]

    [SerializeField] static public UnityEvent<StoreInteractionConfig> OpenStoreEvent = new UnityEvent<StoreInteractionConfig>();

    [SerializeField] private Inventory inventory;
    [SerializeField] private Animator animationController;

    private Interaction interaction;

    #endregion

    #region [ MESSAGES ]

    private void Start()
    {
        interaction = GetComponent<Interaction>();
        interaction.interacted.AddListener(OpenStore);
    }

    #endregion

    #region [ METHODS ]

    private void OpenStore()
    {
        var config = new StoreInteractionConfig();

        config.calllback = new Action<Item>(ChangeClothes);
        config.items = inventory.items.Where(a => a.acquired == true).ToList();
        config.header = "Change Clothes";
        config.ctaText = "Equip";

        OpenStoreEvent.Invoke(config);
        Player.MobilityChange.Invoke(false);
    }


    private void ChangeClothes(Item item)
    {
        //animationController.runtimeAnimatorController = item.data.anim;
    }

    #endregion
}

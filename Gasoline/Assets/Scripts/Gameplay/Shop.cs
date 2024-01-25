using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Shop : MonoBehaviour
{
    #region [ EVENTS ]

    [SerializeField] static public UnityEvent<StoreInteractionConfig> OpenStoreEvent = new UnityEvent<StoreInteractionConfig>();

    #endregion

    #region [ VARIABLES ]
    [SerializeField] private Inventory inventory;

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

        config.calllback = new Action<Item>(Purchase);
        config.items = inventory.items;
        config.header = "Buy Clothes";
        config.ctaText = "Buy";

        OpenStoreEvent.Invoke(config);
        Player.MobilityChange.Invoke(false);
    }


    private void Purchase(Item item)
    {
        int index = inventory.items.FindIndex(a => a.data == item.data);

        item.acquired = true;
        inventory.items[index] = item;
    }

    #endregion
}

#region [ STRUCTS ]

[Serializable]
public struct StoreInteractionConfig
{
    [SerializeField] internal Action<Item> calllback;
    [SerializeField] internal List<Item> items;
    [SerializeField] internal string header;
    [SerializeField] internal string ctaText;
}

#endregion


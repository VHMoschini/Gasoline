using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SaveItem;

[CreateAssetMenu(fileName = "PlayerDatabase", menuName = "ScriptableObjects/PlayerDatabase", order = 2)]
public class PlayerDatabase : ScriptableObject
{
    [SerializeField] private static List<SaveItem> inventory = new List<SaveItem>();
    [SerializeField] private static List<SaveItem> equipedItems = new List<SaveItem>();

    public static void AddInventory(ItemData item)
    {
        SaveItem newItem = new SaveItem();
        newItem.itemType = item.type;
        newItem.data = item;

        inventory.Add(newItem);
    }


    public static void EquipItem(ItemData item)
    {
        SaveItem savedItem = equipedItems.Find(a => a.itemType == item.type);
        equipedItems.Remove(savedItem);

        SaveItem newItem = new SaveItem();
        newItem.itemType = item.type;
        newItem.data = item;

        equipedItems.Add(newItem);

        Debug.Log(equipedItems.Count);
    }

    public static List<ItemData> GetItemsByType(ItemType type)
    {
        var items = inventory.FindAll(a => a.itemType == type);

        return items
                .Select(x => x.data)
                .ToList();
    }

    public static ItemData GetEquipedItemsByType(ItemType type)
    {
        var items = equipedItems.Find(a => a.itemType == type);


        if (items.data == null)
        {
            return null;
        }

        return items.data;               
    }

}


[Serializable]
public struct SaveItem
{
    public enum ItemType
    {
        Wheel,
        Bumper,
        Spoiler,
        Color
    }

    [SerializeField] internal ItemType itemType;
    [SerializeField] internal ItemData data;
}
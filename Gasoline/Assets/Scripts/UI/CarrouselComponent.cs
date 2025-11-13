using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CarrouselComponent : MonoBehaviour
{
    #region [ VARIABLES ]

    enum direction
    {
        LEFT,
        RIGHT
    }

    [SerializeField] private List<CarrouselItem> carrouselItems;
    [SerializeField] private GameObject item;
    [SerializeField] private RectTransform[] points;
    [SerializeField] private PoolInstantiator instantiator;
    [SerializeField] private MeshRenderer renderer;

    [Space(10)]
    [SerializeField] private GameObject signTag;
    [SerializeField] internal List<Item> items;
    [SerializeField] internal bool isStore = false;
    private int selectedItemIndex;



    #endregion

    #region [ UNITY METHODS ]

    private void OnEnable()
    {
        CreateItems();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            RollItems(direction.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            RollItems(direction.RIGHT);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.End)  && isStore)
        {
            int i = items.FindIndex(x => x.data == GetSelectedItem().data);
            Item item = new Item { data = items[i].data, acquired = true };
            items[i] = item;

            PlayerDatabase.AddInventory(items[i].data);

            signTag.SetActive(true);
            InitializeItems(items, carrouselItems);

        }
    }

    #endregion

    #region [ METHODS ]

    private void CreateItems()
    {
        CleanItems();
        carrouselItems = new List<CarrouselItem>();

        if (!isStore && !items[0].data.isColor)
        {
            var itemDatas = PlayerDatabase.GetItemsByType(items[0].data.type);
            if (itemDatas.Count == 0)
            {
                return;
            }
        }

        foreach (var i in points)
        {
            Transform parent = points[0].transform.parent;

            var _item = Instantiate(item, i.position, i.rotation, parent);
            _item.transform.localScale = i.localScale;

            carrouselItems.Add(_item.GetComponent<CarrouselItem>());

        }

        InitializeItems(items, carrouselItems);
        selectedItemIndex = points.Length / 2;

        if (isStore)
        {
            var i = GetSelectedItem();
            instantiator.instantiate(i.data.showcaseObj);

            signTag.SetActive(i.acquired);
        }

    }

    private void InitializeItems(List<Item> _items, List<CarrouselItem> _carrouselItems)
    {
        var itemsToInitialize = _items;

        if (!isStore && !_items[0].data.isColor)
        {
            var itemDatas = PlayerDatabase.GetItemsByType(items[0].data.type);

            if (itemDatas == null)
            {
                CleanItems();
                carrouselItems = null;
                return;
            }

            itemsToInitialize = itemDatas.Select(x => new Item() { data = x }).ToList();
        }

        int itemIndex = 0;

        foreach (var item in _carrouselItems)
        {
            if (itemIndex == itemsToInitialize.Count) itemIndex = 0;

            item.initializeItem(itemsToInitialize[itemIndex]);
            itemIndex++;
        }
    }

    public void RollItemsPublic(bool toLeft)
    {
        if (toLeft)
        {
            RollItems(direction.LEFT);
        }
        else
        {
            RollItems(direction.RIGHT);
        }
    }

    private void RollItems(direction _direction)
    {
        if (carrouselItems.Count == 0) return;

        CarrouselItem itemToMove = null;
        int newIndex = 0;

        switch (_direction)
        {
            case direction.LEFT:

                itemToMove = carrouselItems.First();
                newIndex = carrouselItems.Count;

                break;
            case direction.RIGHT:

                itemToMove = carrouselItems.Last();
                newIndex = 0;

                break;
        }

        carrouselItems.Move(itemToMove, newIndex);

        for (int i = 0; i < carrouselItems.Count; i++)
        {
            if (carrouselItems[i] == itemToMove)
            {
                carrouselItems[i].manipulateItem(points[i], false);
            }
            else
            {
                carrouselItems[i].manipulateItem(points[i]);
            }
        }

        if (GetSelectedItem().data.showcaseObj && isStore)
        {
            var i = GetSelectedItem();
            instantiator.instantiate(i.data.showcaseObj);

            signTag.SetActive(i.acquired);

        }

        if (GetSelectedItem().data.carObject && !isStore)
        {
            instantiator.instantiate(GetSelectedItem().data.carObject);
            PlayerDatabase.EquipItem(GetSelectedItem().data);
        }

        if (GetSelectedItem().data.colorMaterial && GetSelectedItem().data.isColor)
        {
            renderer.material = GetSelectedItem().data.colorMaterial;
            PlayerDatabase.EquipItem(GetSelectedItem().data);
        }
    }

    internal Item GetSelectedItem()
    {
        var item = carrouselItems[selectedItemIndex];
        return item.itemConfig;
    }

    internal void CleanItems()
    {
        if (carrouselItems == null) return;

        foreach (var item in carrouselItems)
        {
            Destroy(item.gameObject);
        }

        carrouselItems.Clear();
    }

    #endregion


}

#region [ STRUCTS ]

[Serializable]
public struct Item
{
    [SerializeField] internal ItemData data;
    [SerializeField] internal bool acquired;
    [SerializeField] internal bool showPrice;

}

#endregion

public static class ListExtension
{
    public static void Move<T>(this List<T> list, T item, int newIndex)
    {
        if (item != null)
        {
            var oldIndex = list.IndexOf(item);
            if (oldIndex > -1)
            {
                list.RemoveAt(oldIndex);

                if (newIndex > oldIndex) newIndex--;

                list.Insert(newIndex, item);
            }
        }
    }
}




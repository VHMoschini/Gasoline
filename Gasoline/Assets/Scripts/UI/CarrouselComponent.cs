using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            //RollItems(1);

            RollItems(direction.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            //RollItems(-1);
            RollItems(direction.RIGHT);

        }
    }

    #endregion

    #region [ METHODS ]

    private void CreateItems()
    {
        CleanItems();
        carrouselItems = new List<CarrouselItem>();

        foreach (var i in points)
        {

            Transform parent = points[0].transform.parent;

            var _item = Instantiate(item, i.position, i.rotation, parent);
            _item.transform.localScale = i.localScale;

            carrouselItems.Add(_item.GetComponent<CarrouselItem>());

        }

        InitializeItems(items, carrouselItems);
        selectedItemIndex = carrouselItems.Count / 2;

        if (GetSelectedItem().data.showcaseObj && isStore)
            instantiator.instantiate(GetSelectedItem().data.showcaseObj);

    }

    private void InitializeItems(List<Item> _items, List<CarrouselItem> _carrouselItems)
    {
        int itemIndex = 0;

        foreach (var item in _carrouselItems)
        {
            if (itemIndex == _items.Count) itemIndex = 0;

            item.initializeItem(_items[itemIndex]);
            itemIndex++;
        }
    }

    private void RollItems(direction _direction)
    {

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
            instantiator.instantiate(GetSelectedItem().data.showcaseObj);

        if (GetSelectedItem().data.carObject && !isStore)
            instantiator.instantiate(GetSelectedItem().data.carObject);

        if (GetSelectedItem().data.colorMaterial && GetSelectedItem().data.isColor)
        {
            renderer.material = GetSelectedItem().data.colorMaterial;
        }
    }

    internal Item GetSelectedItem()
    {
        var item = carrouselItems[selectedItemIndex];
        //item.ChooseItem();

        return item.itemConfig;
    }

    internal void CleanItems()
    {
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




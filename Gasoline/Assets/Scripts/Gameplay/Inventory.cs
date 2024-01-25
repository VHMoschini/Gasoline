using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region [ DATABASE ]

    [SerializeField]  internal List<Item> items = new List<Item>();

    internal Item GetItem(Item item)
    {
        foreach (var i in items)
        {
            if (i.data == item.data)
            {
                return i;
            }
        }

        return items.FirstOrDefault();
    }

    #endregion
}

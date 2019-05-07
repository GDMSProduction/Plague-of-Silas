/*
-Author: Joseph Gill
-Date: 05/05/2019
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTableScript : MonoBehaviour
{
    public Dictionary<string, GameObject> itemTable = new Dictionary<string, GameObject>();
    public List<GameObject> items = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < items.Count; ++i)
        {
            itemTable.Add(items[i].name, items[i]);
        }
    }

    public GameObject FindItem(string name)
    {
        return itemTable[name];
    }
}

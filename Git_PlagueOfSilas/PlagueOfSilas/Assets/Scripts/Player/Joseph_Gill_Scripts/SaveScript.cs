/*
-Author: Joseph Gill
-Date: 05/05/2019
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveScript : MonoBehaviour
{
    ItemToSave itemToSave;
    GameObject item;
    // Start is called before the first frame update
    void Start()
    {
        item = GameObject.FindWithTag("CanSave");
        itemToSave = item.GetComponent<ItemToSave>();
    }

    // Update is called once per frame
    void Update()
    {        
    }

    public void SaveItem()
    {

        //Stores the items id number
        PlayerPrefs.SetString("Name", itemToSave.itemName);

        //Stores the items X postion in the world
        PlayerPrefs.SetFloat("xPos", itemToSave.pos.x);

        //Stores the items Y postion in the world
        PlayerPrefs.SetFloat("yPos", itemToSave.pos.y);

        //Stores the items Z postion in the world
        PlayerPrefs.SetFloat("zPos", itemToSave.pos.z);

        if (PlayerPrefs.HasKey("ID"))
        {
            if (PlayerPrefs.GetString("Name", " ") == itemToSave.itemName)
            {
                Debug.Log("Item successfully saved");
                Object.Destroy(item);
            }
        }

        else
        {
            Debug.Log("Item failed to save");
        }
    }
}

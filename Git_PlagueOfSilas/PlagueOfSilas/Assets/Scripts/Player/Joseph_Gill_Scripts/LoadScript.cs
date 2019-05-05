/*
-Author: Joseph Gill
-Date: 05/05/2019
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScript : MonoBehaviour
{
    GameObject item;
    ItemToSave itemToSave;
    ItemTableScript itemTable;
    string itemName;
    float posX, posY, posZ;
    // Start is called before the first frame update
    void Start()
    {
        item = new GameObject();
        itemToSave = new ItemToSave();
        itemTable = GameObject.FindObjectOfType<ItemTableScript>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void LoadItem()
    {

        //Recieve the saved items id number
        itemName = PlayerPrefs.GetString("Name", "");

        //Recieve the saved items X postion in the world
        posX = PlayerPrefs.GetFloat("xPos", 0);

        //Recieve the saved items Y postion in the world
        posY = PlayerPrefs.GetFloat("yPos", 0);

        //Recieve the saved items Z postion in the world
        posZ = PlayerPrefs.GetFloat("zPos", 0);

        //Set the id in the ItemToSave script equal to the id that was read in from the PlayerPref
        itemToSave.itemName = itemName;
        
        //Set the X position in the ItemToSave script equal to the X position that was read in from the PlayerPref
        itemToSave.pos.x = posX;

        //Set the Y position in the ItemToSave script equal to the Y position that was read in from the PlayerPref
        itemToSave.pos.y = posY;

        //Set the Z position in the ItemToSave script equal to the Z position that was read in from the PlayerPref
        itemToSave.pos.z = posZ;

        itemToSave.sliderX = GameObject.FindWithTag("SliderX").GetComponent<Slider>();
        itemToSave.sliderY = GameObject.FindWithTag("SliderY").GetComponent<Slider>();
        itemToSave.sliderZ = GameObject.FindWithTag("SliderZ").GetComponent<Slider>();

        if (PlayerPrefs.GetString("Name", "") == itemName)
        {
            Debug.Log("Item successfully Loaded");
            Object.Instantiate(itemTable.FindItem(itemName));
        }

        else
        {
            Debug.Log("Item failed to save");
        }
    }
}

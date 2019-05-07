/*
-Author: Joseph Gill
-Date: 05/05/2019
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToSave : MonoBehaviour
{
    [HideInInspector] public string itemName;
    [HideInInspector] public Vector3 pos;
    [HideInInspector] public Slider sliderX, sliderY, sliderZ;

    // Start is called before the first frame update
    void Start()
    {
        //ID will be used to find the item when loading
        itemName = this.name;
        //POS is a vector3 of the items position I am using to test saving and loading different values
        pos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        //SliderX/Y/Z are used to change the position of the temp item
        sliderX = GameObject.FindWithTag("SliderX").GetComponent<Slider>();
        sliderY = GameObject.FindWithTag("SliderY").GetComponent<Slider>();
        sliderZ = GameObject.FindWithTag("SliderZ").GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        //pos.x = sliderX.value;
        //pos.y = sliderY.value;
        //pos.z = sliderZ.value;

        this.transform.position = pos;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmScript : MonoBehaviour {

    Light lamp;

	// Use this for initialization
	void Start ()
    {
        lamp = GetComponentInChildren<Light>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.E))
        {
            ToggleLight();
        }
	}

    void ToggleLight()
    {
        if (lamp.enabled)
            lamp.enabled = false;

        else
            lamp.enabled = true;
    }
}

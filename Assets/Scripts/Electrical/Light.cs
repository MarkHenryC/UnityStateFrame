using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Light : Resistor 
	{
        public DynamicMaterial lightbulb;
        public GameObject pointLight;
        public GameObject bulbUnlit, bulbLit;

        public override void Activate(bool on = true)
        {
            base.Activate(on);

            if (on)
                Debug.Log("Light on.");

            bulbUnlit.SetActive(!on);
            bulbLit.SetActive(on);
        }
    }
}
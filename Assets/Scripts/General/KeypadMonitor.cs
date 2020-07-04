using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class KeypadMonitor : MonoBehaviour 
	{
        public SwitchGrid keypad;

        private string composite;

		void Awake () 
		{
            keypad.OnChildSelection = OnSelectedKey;
		}
		
		void Start () 
		{
            composite = "";
        }	

        private void OnSelectedKey(SwitchGridItem key)
        {
            string s = key.name;
            if (s == "C")
                composite = "";
            else if (s == "#")
            {
                if (int.TryParse(composite, out int code))
                {
                    Debug.LogFormat("Parsed code as {0}", code);

                    ActivitySettings.Asset.overrideCode = code;
                }
                else
                    Debug.LogFormat("Failed to parse entered code {0}", s);

                composite = "";
            }
            else
                composite += s;
        }
	}
}
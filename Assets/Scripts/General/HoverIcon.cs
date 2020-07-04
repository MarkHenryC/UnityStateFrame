using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    /// <summary>
    /// An icon (sprite or model) will
    /// appear at a specified distance
    /// from its host object
    /// </summary>
	public class HoverIcon : MonoBehaviour 
	{
        public GameObject icon;
        public Vector3 distanceFromCentre;
        public bool showByDefault;
        public bool lookAtCamera;

        private bool isActive;

		void Awake () 
		{
            Show(showByDefault);
        }

        private void Update()
        {
            if (lookAtCamera && icon && isActive)
                Utils.FaceCamera(icon.transform);
        }

        public void Show(bool show)
        {            
            if (icon)
            {
                isActive = show;
                icon.SetActive(isActive);
                icon.transform.position = transform.position + distanceFromCentre;
            }
        }
    }
}
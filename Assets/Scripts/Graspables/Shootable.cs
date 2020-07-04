using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Shootable : Graspable 
	{
        public int id;

        public bool Shot { get; private set; }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            info.GrabbedObject = null;
            Shot = true;
            clickDownAction?.Invoke(this);
        }

        public override bool IsButton()
        {
            return true;
        }

        /// <summary>
        /// Overridden because we may not be
        /// at the top level of the prefab
        /// </summary>
        /// <param name="show"></param>
        public override void Show(bool show)
        {
            transform.parent.gameObject.SetActive(show);
        }
    }
}
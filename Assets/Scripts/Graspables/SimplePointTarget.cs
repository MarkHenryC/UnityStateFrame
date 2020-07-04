using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class SimplePointTarget : ButtonPanel 
	{
        [Tooltip("Optional: a visual cue when pointed at")]
        public GameObject pointerCue;
        [Tooltip("Optional: are we part of a collection")]
        public GrabbableManager manager;

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            pointerCue.SetMode(true);
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            pointerCue.SetMode(false);
        }
    }
}
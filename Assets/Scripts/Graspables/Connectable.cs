using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Connectable : Placeable 
	{
        public Action<Connectable> CallOnClick, CallOnRelease;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);

            CallOnClick?.Invoke(this);
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            CallOnRelease?.Invoke(this);
        }
    }
}
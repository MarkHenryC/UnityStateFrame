using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class SwitchGridItem : Graspable
	{
        private SwitchGrid switchGrid;

        protected override void Awake()
        {
            base.Awake();

            switchGrid = GetComponentInParent<SwitchGrid>();
        }

        public override bool IsButton()
        {
            return true;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            if (switchGrid)
                switchGrid.OnChildSelection?.Invoke(this);
        }
    }
}
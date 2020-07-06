using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    public class PowerSource : Block
    {
        public Connection live, neutral;

        protected override void Awake()
        {
            base.Awake();

            live.OnConnectionChanged = ConnectionChanged;
            neutral.OnConnectionChanged = ConnectionChanged;
        }
    }
}
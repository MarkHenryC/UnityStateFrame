using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    /// <summary>
    /// Single pole, double throw.
    /// Leave default output L1 disconnected
    /// to use as on-off switch
    /// </summary>
    public class Switch : Block
    {
        public Connection common, L1, L2;

        public Action<bool, Switchable> NotifyOnSwitch;

        public Connection ConnectedToCommon { private set; get; }
        public bool IsUp { get; private set; }

        private Switchable switchable;

        protected override void Awake()
        {
            base.Awake();

            switchable = GetComponent<Switchable>();
            if (switchable)
                switchable.CallOnSwitchId = SetSwitch;

            common.OnConnectionChanged = ConnectionChanged;
            L1.OnConnectionChanged = ConnectionChanged;
            if (L2)
                L2.OnConnectionChanged = ConnectionChanged;

            SetSwitch();
        }

        public void SetSwitch(bool up = true, Switchable source = null)
        {
            IsUp = up;

            ConnectedToCommon = up ? L1 : L2;
            NotifyOnSwitch?.Invoke(up, source);

            UpdateConnection(Circuit.TraceTrigger.Switch);
        }

        public override void Bind(Connection c)
        {
            base.Bind(c);
            c.Next = GetNext;
        }

        private Connection GetNext(Connection source)
        {
            if (source == common && ConnectedToCommon)
                return ConnectedToCommon.to ?? ConnectedToCommon.From;
            else if (source == ConnectedToCommon && common)
                return common.to ?? common.From;
            else
                return null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Resistor : Block
	{
        public Connection a, b;

        protected override void Awake()
        {
            base.Awake();

            a.OnConnectionChanged = ConnectionChanged;
            b.OnConnectionChanged = ConnectionChanged;
        }

        public virtual void Activate(bool on = true)
        {
            Debug.LogFormat("Resistor {0} activated? {1}.", gameObject.name, on ? "Yes" : "No");
        }

        public override void Bind(Connection c)
        {
            base.Bind(c);
            c.Next = GetNext;
        }

        private Connection GetNext(Connection source)
        {
            if (source == a && b)
                return b.to ?? b.From;
            else if (source == b && a)
                return a.to ?? a.From;
            else
                return null;
        }
    }
}
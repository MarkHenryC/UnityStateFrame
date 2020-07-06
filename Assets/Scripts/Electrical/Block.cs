using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Block : MonoBehaviour 
	{
        public Circuit circuit;
        public BlockType blockType;

        public Action<Connection> OnConnectionChanged;

        public enum BlockType { Direct, Resistor, Open, Power }

        public virtual void UpdateConnection(Circuit.TraceTrigger trigger)
        {
            if (circuit)
                circuit.Trace(trigger);
        }

        public virtual void Bind(Connection c)
        {

        }
	
		void Start () 
		{
			
		}

        protected virtual void Awake()
        {
        }

        protected virtual void ConnectionChanged(Connection target)
        {
            OnConnectionChanged?.Invoke(target);
        }
    }
}
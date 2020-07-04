using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{    
    public class Circuit : MonoBehaviour
    {
        public PowerSource powersource;        
        public GameObject shortCircuit;
        public List<Resistor> resistors = new List<Resistor>();
        public CircuitType circuitType;

        public Action<CircuitType, TraceTrigger> CallOnTraceComplete;

        public enum CircuitType { Open, Closed, Short, Incomplete };
        public enum TraceTrigger { Test, Switch, Rewire }

        private void OnDisable()
        {
            shortCircuit.SetActive(false);
        }

        public void Connect(Connection c1, Connection c2)
        {
            c1.to = c2;
            c2.to = c1;
        }

        public void Disconnect(Connection c)
        {
            c.to = null;
        }

        public void Trace(TraceTrigger trigger = TraceTrigger.Test)
        {
            foreach (var v in resistors)
                v.Activate(false);

            resistors.Clear();
            circuitType = CircuitType.Open;
            Trace(powersource.live, trigger);
            
            foreach (var v in resistors)
                v.Activate(circuitType == CircuitType.Closed);

            Debug.LogFormat("Circuit result: {0}", circuitType);

            shortCircuit.SetActive(circuitType == CircuitType.Short);

            CallOnTraceComplete?.Invoke(circuitType, trigger);
        }

        private void Trace(Connection c, TraceTrigger trigger)
        {           
            if (c)
            {                
                Resistor r = c.container as Resistor;

                if (r)
                    resistors.Add(r);

                Connection next = c.GetNext();
                if (next)
                {
                    if (next.container == powersource)
                    {
                        if (resistors.Count == 0)
                            circuitType = CircuitType.Short;
                        else
                            circuitType = CircuitType.Closed;

                        return;
                    }
                    else
                    {
                        Trace(next, trigger);
                    }
                }
            }
        }
    }
}
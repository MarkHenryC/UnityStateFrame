using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    [CreateAssetMenu()]
    public class ActionFloatProvider : ScriptableObject
    {
        private List<Action<IUnitValueProvider, float>> listeners = new List<Action<IUnitValueProvider, float>>();

        public void AddResponder(Action<IUnitValueProvider, float> a)
        {
            if (!listeners.Contains(a))
                listeners.Add(a);
        }

        public void RemoveResponder(Action<IUnitValueProvider, float> a)
        {
            if (listeners.Contains(a))
                listeners.Remove(a);
        }

        public void Invoke(IUnitValueProvider p, float f)
        {
            foreach (var l in listeners)
                l.Invoke(p, f);
        }
    }    
}
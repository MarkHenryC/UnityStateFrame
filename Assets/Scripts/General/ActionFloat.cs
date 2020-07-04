using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    [CreateAssetMenu()]
    public class ActionFloat : ScriptableObject
    {
        private List<Action<float>> listeners = new List<Action<float>>();

        public void AddResponder(Action<float> a)
        {
            if (!listeners.Contains(a))
                listeners.Add(a);
        }

        public void RemoveResponder(Action<float> a)
        {
            if (listeners.Contains(a))
                listeners.Remove(a);
        }

        public void Invoke(float f)
        {
            foreach (var l in listeners)
                l.Invoke(f);
        }
    }    
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class AnimEventReceiver : MonoBehaviour 
	{
        public Action AnimEvent;
        public Action<float> AnimEventFloat;
        public Action<int> AnimEventInt;
        public Action<string> AnimEventString;
        public Action<UnityEngine.Object> AnimEventObject;

        public void EventVoid()
        {
            AnimEvent?.Invoke();
        }

        public void EventFloat(float f)
        {
            AnimEventFloat?.Invoke(f);
        }

        public void EventInt(int i)
        {
            AnimEventInt?.Invoke(i);
        }

        public void EventString(string s)
        {
            AnimEventString?.Invoke(s);
        }

        public void EventObject(UnityEngine.Object o)
        {
            AnimEventObject?.Invoke(o);
        }
	}
}
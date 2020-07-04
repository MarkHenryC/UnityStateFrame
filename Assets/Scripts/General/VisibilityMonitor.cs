using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class VisibilityMonitor : MonoBehaviour 
	{
        public Action<bool> VisibilityChanged;

        private void OnBecameInvisible()
        {
            VisibilityChanged?.Invoke(false);
        }

        private void OnBecameVisible()
        {
            VisibilityChanged?.Invoke(true);
        }
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class DelayedDestroy : MonoBehaviour 
	{
        public float destructionTime;

        private float counter;

        private void Update()
        {
            counter += Time.deltaTime;
            if (counter >= destructionTime)
                Destroy(gameObject);
        }
    }
}
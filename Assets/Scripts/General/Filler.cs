using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Filler : MonoBehaviour 
	{
        [Tooltip("Normalized scale based on current scale. Set transform to max size")]
        public Vector3 minScale, maxScale;
        [Tooltip("Fullness 0 .. 1")]
        public float initialLevel;

        private MeshRenderer mr;
        private Vector3 defaultScale;

		void Awake () 
		{
            defaultScale = transform.localScale;
            mr = GetComponent<MeshRenderer>();
            Fill(initialLevel);
		}

        public void Fill(float unitVal)
        {
            if (unitVal == 0f)
                mr.enabled = false; // Hide since we're empty
            else
                mr.enabled = true;
            Vector3 scale = Vector3.Lerp(minScale, maxScale, Mathf.Clamp01(unitVal));
            transform.localScale = Vector3.Scale(defaultScale, scale);
        }
	}
}
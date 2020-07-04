using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Moveable : MonoBehaviour 
	{
        public Vector3 startPosition, endPosition;

		void Awake () 
		{
			
		}
		
		void Start () 
		{
			
		}	

        public void SetPosition(float t)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
        }

        public void SetStartPosition()
        {
            startPosition = transform.position;
        }

        public void SetEndPosition()
        {
            endPosition = transform.position;
        }
	}
}
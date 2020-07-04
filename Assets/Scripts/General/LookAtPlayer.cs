using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class LookAtPlayer : MonoBehaviour 
	{
        public Transform target;

        protected virtual void Update()
        {
            if (target)
                Utils.FaceCamera(transform, target);
            else
                Utils.FaceCamera(transform, ControllerInput.Instance.Player);
        }
    }
}
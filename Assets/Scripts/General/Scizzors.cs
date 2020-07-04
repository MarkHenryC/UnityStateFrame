using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DentedPixel;

namespace QS
{
	public class Scizzors : MonoBehaviour 
	{
        public Action<GameObject, bool> CollisionCall;
        public GameObject leftBlade, rightBlade;
        public float restAngleLeft, restAngleRight, cutAngleLeft, cutAngleRight;
        public float snipTime = .5f;

        private Vector3 restAnglesLeft, restAnglesRight, cutAnglesLeft, cutAnglesRight;

        private void Awake()
        {
            cutAnglesLeft = restAnglesLeft = leftBlade.transform.localEulerAngles;
            cutAnglesRight = restAnglesRight = rightBlade.transform.localEulerAngles;

            cutAnglesLeft.z = cutAngleLeft;
            cutAnglesRight.z = cutAngleRight;
        }

        void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Scizzors OnTriggerEnter: " + other.name);

            CollisionCall?.Invoke(other.gameObject, true);
        }

        void OnTriggerExit(Collider other)
        {
            //Debug.Log("Cuttable OnTriggerExit: " + other.name);

            CollisionCall?.Invoke(other.gameObject, false);
        }

        public void Cut()
        {
            LeanTween.rotateLocal(leftBlade, cutAnglesLeft, snipTime).setOnComplete(
                () => { LeanTween.rotateLocal(leftBlade, restAnglesLeft, snipTime); }
                );
            LeanTween.rotateLocal(rightBlade, cutAnglesRight, snipTime).setOnComplete(
                () => { LeanTween.rotateLocal(rightBlade, restAnglesRight, snipTime); }
                );
        }
    }
}
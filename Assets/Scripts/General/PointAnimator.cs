using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;

namespace QS
{
    public class PointAnimator : MonoBehaviour
    {
        public Vector3 relativeMove;
        public float timespan;

        private Vector3 origPos;

        private void Awake()
        {
            origPos = transform.position;
            LeanTween.move(gameObject, origPos + relativeMove, timespan).setLoopPingPong();
        }

        void Update()
        {

        }
    }
}
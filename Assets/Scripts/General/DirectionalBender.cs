using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    public class DirectionalBender : MonoBehaviour
    {
#if USING_MEGABEND
        public MegaBend bender;
#endif
        public float maxAbsAngle;
        public float speedThreshold = .5f;
        public float speedMax = .15f;

        private Vector3 prevPosition;
        private float[] velocities;
        private float speedRange;

        private const int SmoothingSamples = 5;

        void Awake()
        {
            prevPosition = transform.position;
            velocities = new float[SmoothingSamples];
            speedRange = speedMax - speedThreshold;
        }

        void Update()
        {
            Vector3 direction = transform.position - prevPosition;
            float angle = Vector3.Angle(transform.right, direction);
            float broomHeadDirection = transform.rotation.eulerAngles.y;

#if USING_MEGABEND
            int multiplier = 0;
            if (angle > 315 || angle < 45)
                multiplier = 1;
            else if (angle > 135 && angle < 225)
                multiplier = -1;
#endif
            float distance = Vector3.Magnitude(direction);
            prevPosition = transform.position;
            float avg = Utils.BucketShiftLeft(velocities, distance, SmoothingSamples);
            if (avg >= speedThreshold)
            {
                float limit = Mathf.Min(avg, speedMax);
                float norm = (limit - speedThreshold) / speedRange;
#if USING_MEGABEND
                bender.angle = maxAbsAngle * norm * multiplier;
#endif

            }
#if USING_MEGABEND
            else
                bender.angle = 0;
#endif
        }
    }
}
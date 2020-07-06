using UnityEngine;
using System;

namespace QS
{
    /// <summary>
    /// Used for pushing gas or liquid
    /// </summary>
	public class Blower : MonoBehaviour
    {
        public Blowable blowable;
        public LayerMask blowableMask;
        public bool continuous, trigger;
        public float force = 5f;
        public float taperTime = .5f;
        public float range = 1f;
        public bool targeted = true; // if not, just a directional wind
        public GameObject effect;

        public Action<float> ReportForce;

        protected float forceCoefficient;
        protected bool isBlowing;

        private void Update()
        {
            if (continuous || trigger)
            {
                if (effect)
                    effect.SetActive(true);
                isBlowing = true;
                ProjectWind();
            }
            else if (isBlowing)
            {
                blowable.EndBlow(taperTime);
                isBlowing = false;
                if (effect)
                    effect.SetActive(false);
            }

            if (isBlowing && ReportForce != null)
                ReportForce(force * forceCoefficient);
        }

        private void ProjectWind()
        {
            bool onTarget = (!targeted || OnTarget());
            if (onTarget)
            {
                blowable.Blow(transform.forward, force * forceCoefficient);
            }
            else if (isBlowing)
            {
                blowable.EndBlow(taperTime);
            }
        }

        private bool OnTarget()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range, blowableMask))
            {
                forceCoefficient = 1f + (range - hit.distance);
                return true;
            }
            return false;
        }
    }
}
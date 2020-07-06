using System.Collections;
using UnityEngine;

namespace QS
{
    public class Blowable : MonoBehaviour
    {
        public float damping = .25f;

        private Vector3 currentDirection;
        private Cloth[] strands; // items to be blowin' in the wind
        private bool initialized;

        private void Start()
        {
            if (!initialized)
            {
                Initialize();
                SetDamping(damping);
            }
        }

        /// <summary>
        /// Public access for when meshes are created
        /// procedurally, as we can't be sure they'll
        /// be ready on Start()
        /// </summary>
        public void Initialize()
        {
            if (strands == null || strands.Length == 0)
                strands = GetComponentsInChildren<Cloth>();            
            initialized = true;
        }

        public void Clear()
        {
            strands = null;
        }

        public void Blow(Vector3 direction, float force = 1f)
        {
            if (strands == null)
                return;

            currentDirection = direction * force;
            foreach (Cloth cloth in strands)
                cloth.externalAcceleration = currentDirection;
        }

        public void EndBlow(float taperTime)
        {
            if (strands == null)
                return;

            if (taperTime > 0f)
                StartCoroutine(TaperBlow(taperTime));
            else
            {
                currentDirection = Vector3.zero;
                foreach (Cloth cloth in strands)
                    cloth.externalAcceleration = currentDirection;
            }
        }

        public void SetDamping(float d)
        {
            if (strands == null)
                return;

            foreach (Cloth cloth in strands)
                cloth.damping = d;
        }

        private IEnumerator TaperBlow(float time)
        {
            float t = time;
            while (t > 0f)
            {
                currentDirection = Vector3.Lerp(currentDirection, Vector3.zero, 1f - t / time);
                foreach (Cloth cloth in strands)
                    cloth.externalAcceleration = currentDirection;

                t -= Time.deltaTime;

                yield return null;
            }
        }
    }
}
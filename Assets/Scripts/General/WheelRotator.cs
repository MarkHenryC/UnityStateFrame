using UnityEngine;

namespace QS
{
    public class WheelRotator : MonoBehaviour
    {
        public Transform[] wheels;
        public Vector3 rotationAxis;
        public float rotationAnglesPerSecond;

        private Vector3 previousPosition, direction;
        private float currentRotation;

        private void Awake()
        {
            previousPosition = transform.localPosition;
        }

        private void Update()
        {
            Vector3 currentPosition = transform.localPosition;
            int dir = 0;
            if (currentPosition.z < previousPosition.z)
                dir = 1;
            else if (currentPosition.z > previousPosition.z)
                dir = -1;

            //Debug.Log(transform.localPosition.z);
            //Debug.Log(dir);
            currentRotation += rotationAnglesPerSecond * Time.deltaTime * dir;
            for (int i = 0; i < wheels.Length; i++)
                wheels[i].localRotation = Quaternion.AngleAxis(currentRotation, rotationAxis);

            previousPosition = currentPosition;
        }
    }
}
using System;
using UnityEngine;

namespace QS
{
    public class TriggerNotify : MonoBehaviour
    {
        public Action<GameObject> OnTrigger, OnExitTrigger;
        public Action<GameObject> OnCollide, OnExitCollide;

        private void OnTriggerEnter(Collider other)
        {
            OnTrigger?.Invoke(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnCollide?.Invoke(collision.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            OnExitTrigger?.Invoke(other.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            OnExitCollide?.Invoke(collision.gameObject);
        }

    }
}
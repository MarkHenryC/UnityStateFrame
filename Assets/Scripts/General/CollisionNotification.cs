using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace QS
{
    public class CollisionNotification : MonoBehaviour 
	{
        public UnityEvent OnCollide, OnTrigger;
        public UnityEvent UeCollideExit, UeTriggerExit;
        public Action<GameObject> OnCollideObject, OnTriggerObject;
        public Action<GameObject> OnCollideObjectExit, OnTriggerObjectExit;

        public string collisionName, triggerName, collisionExitName, triggerExitName;

        private void OnCollisionEnter(Collision collision)
        {
            if (OnCollide != null)
            {
                if (collisionName.Usable())
                {
                    if (collisionName == collision.gameObject.name)
                        OnCollide.Invoke();
                }
                else
                    OnCollide.Invoke();
            }

            if (OnCollideObject != null)
                OnCollideObject(collision.gameObject);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (OnTrigger != null)
            {
                if (triggerName.Usable())
                {
                    if (triggerName == other.name)
                        OnTrigger.Invoke();
                }
                else
                    OnTrigger.Invoke();
            }

            if (OnTriggerObject != null)
                OnTriggerObject(other.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (UeCollideExit != null)
            {
                if (collisionExitName.Usable())
                {
                    if (collisionExitName == collision.gameObject.name)
                        UeCollideExit.Invoke();
                }
                else
                    UeCollideExit.Invoke();
            }

            if (OnCollideObjectExit != null)
                OnCollideObjectExit(collision.gameObject);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (UeTriggerExit != null)
            {
                if (triggerExitName.Usable())
                {
                    if (triggerExitName == other.name)
                        UeTriggerExit.Invoke();
                }
                else
                    UeTriggerExit.Invoke();
            }

            if (OnTriggerObjectExit != null)
                OnTriggerObjectExit(other.gameObject);
        }

    }
}
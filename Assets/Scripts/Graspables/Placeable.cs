using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DentedPixel;

namespace QS
{
    public class Placeable : Graspable
    {
        public string info = "Release this object to add";
        public Action<Placeable, Receivable, bool> CallOnDrop;
        public Action<Placeable, Receivable> CallOnEnter, CallOnExit, CallOnCollisionEnter, CallOnCollisionExit;
        public Action<Placeable, Hazard> CallOnEnterHazard;
        public float returnTime = 1f;
        public bool returnToHomeIfReleased; // Auto version of OnDrop callback

        protected Vector3 initialPos;
        protected Quaternion initialRot;
        protected bool inProximity, inCollision;
        protected Receivable lastReceived; // Cache this so we can clear highlight when trigger up

        protected override void Start()
        {
            base.Start();
            initialPos = transform.position;
            initialRot = transform.rotation;
        }

        public virtual Receivable LastReceived
        {
            get { return lastReceived; }
        }

        /// <summary>
        /// When we're in range of the receivable,
        /// detected by the receivable's trigger
        /// </summary>
        /// <param name="receivable"></param>
        public virtual void NotifyEnterProximity(Receivable receivable)
        {
            inProximity = true;            
            receivable.SetTriggerInfo(info);
            CallOnEnter?.Invoke(this, receivable);
            lastReceived = receivable;
            Debug.Log("EnterProximity " + receivable.name);
        }

        public virtual void NotifyExitProximity(Receivable receivable)
        {
            inProximity = false;
            CallOnExit?.Invoke(this, receivable);
            lastReceived = null;
            Debug.Log("ExitProximity " + receivable.name);
        }

        public virtual void NotifyEnterCollision(Receivable receivable)
        {
            inCollision = true;
            receivable.SetTriggerInfo(info);
            CallOnCollisionEnter?.Invoke(this, receivable);
            lastReceived = receivable;
            Debug.Log("EnterCollision " + receivable.name);
        }

        public virtual void NotifyExitCollision(Receivable receivable)
        {
            inCollision = false;
            receivable.SetColliderInfo(info);
            CallOnCollisionExit?.Invoke(this, receivable);
            lastReceived = null;
            Debug.Log("ExitCollision " + receivable.name);
        }

        /// <summary>
        /// Such as when dropped outside of zone
        /// </summary>
        public virtual void ReturnToInitialPositionAnimated()
        {            
            Utils.MoveToPosition(this, initialPos, initialRot, returnTime);
        }

        public virtual void ReturnToInitialPosition()
        {
            Utils.MoveToPositionImmediate(this, initialPos, initialRot);
        }

        public virtual void MoveToPosition(Vector3 position, float time = 1.5f)
        {
            Utils.MoveToPositionThenHideAtPosition(this, position, initialPos, time);
        }

        public virtual void MoveAndRotateToPositionWithDelay(Vector3[] path, Vector3 eulerAngles, float delay = 0f, float time = 1.5f)
        {
            LeanTween.delayedCall(delay, () => { MoveAndRotateToPosition(path, eulerAngles, time); });
        }

        public virtual void MoveAndRotateToPosition(Vector3[] path, Vector3 eulerAngles, float time = 1.5f)
        {
            MoveToPosition(path, false, time);
            RotateTo(eulerAngles, time);
        }

        public virtual void MoveToPosition(Vector3[] path, bool orientToPath = true, float time = 1.5f)
        {
            Utils.MoveToPositionAlongPath(this, path, time, orientToPath);
        }

        public virtual void MoveToPositionThenHide(Vector3[] path, bool orientToPath = true, float time = 1.5f)
        {
            Utils.MoveToPositionAlongPathThenHide(this, path, time, orientToPath);
        }

        public virtual void RotateTo(Vector3 eulerAngles, float time = 1.5f)
        {
            Utils.RotateTo(this, eulerAngles, time);
        }

        public virtual void MoveToReceiverPosition()
        {
            MoveToPosition(LastReceived.transform.position);
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            if (CallOnDrop != null)
                CallOnDrop(this, lastReceived, inProximity);

            if (returnToHomeIfReleased)
                ReturnToInitialPosition();

            if (lastReceived)
            {
                lastReceived.SetHighlight(false);
                lastReceived = null;
            }
        }
    }
}

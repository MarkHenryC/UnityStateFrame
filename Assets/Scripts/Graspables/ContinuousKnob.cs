using System;
using UnityEngine;

namespace QS
{
    public class ContinuousKnob : Graspable, IUnitValueProvider
    {
        public Action<float> callOnUpdate;
        public ActionFloatProvider newValueAction;
        public bool ccw = true;
        public bool lockAtZero;

        private Vector3 baseRotation;
        private Vector3 baseRotationController;
        private float rotationAccum;
        private float previousAngle;

        private const float angleEpsilon = 0.01f;

        protected override void Start()
        {
            base.Start();
        }

        public void AddListener(Action<IUnitValueProvider, float> a)
        {
            if (newValueAction)
                newValueAction.AddResponder(a);
        }

        public void RemoveListener(Action<IUnitValueProvider, float> a)
        {
            if (newValueAction)
                newValueAction.RemoveResponder(a);
        }

        public void ResetAll()
        {
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Pass down current controller rotation
        /// </summary>
        /// <param name="rotation"></param>
        public override void UpdateRotation(Quaternion rotation)
        {
            //Debug.LogFormat("{0}:{1}:{2}", rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z);

            float controllerRotationDelta = Utils.DeltaRotationUnitVal(previousAngle, rotation.eulerAngles.z, ccw);

            float prevAccum = rotationAccum;

            rotationAccum += controllerRotationDelta;
            if (lockAtZero && rotationAccum <= 0f)
            {
                rotationAccum = prevAccum;
                return;
            }

            transform.rotation = Quaternion.Euler(baseRotation.x, baseRotation.y, rotationAccum * 360f);

            if (callOnUpdate != null)
                callOnUpdate(controllerRotationDelta);
            if (newValueAction)
                newValueAction.Invoke(this, controllerRotationDelta);

            previousAngle = rotation.eulerAngles.z;
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);

            baseRotation = transform.rotation.eulerAngles;
            baseRotationController = info.ControllerRotation.eulerAngles;
            rotationAccum = 0f;
            previousAngle = baseRotationController.z;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            baseRotation = transform.rotation.eulerAngles;
        }

        public override void HandleHorizontalSwipe(float amount)
        {
            // Nothing to do here
        }

        public override void MoveTo(Vector3 newPos)
        {
            // Not moveable
        }
    }
}
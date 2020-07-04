using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace QS
{
    public class Twistable : Graspable, IUnitValueProvider
    {
        public Action<float> callOnUpdate;
        public ActionFloatProvider newValueAction;

        private Vector3 baseRotation;
        private Vector3 baseRotationController;
        private float previouslVal = 0f;

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
            previouslVal = 0f;
            transform.rotation = Quaternion.identity;
        }

        public override void UpdateRotation(Quaternion rotation)
        {
            Quaternion rot = Quaternion.Euler(baseRotation.x, baseRotation.y, baseRotation.z + rotation.eulerAngles.z - baseRotationController.z);
            float unitVal = 0f;

            if (rot.eulerAngles.z > angleEpsilon)
                unitVal = 1f - (rot.eulerAngles.z / 360f);

            if (unitVal > 0f && !Crossover(unitVal))
            {
                transform.rotation = rot;

                callOnUpdate?.Invoke(unitVal);
                if (newValueAction)
                    newValueAction.Invoke(this, unitVal);

                previouslVal = unitVal;
            }
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);

            baseRotation = transform.rotation.eulerAngles;
            baseRotationController = info.ControllerRotation.eulerAngles;
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

        private bool Crossover(float unitVal)
        {
            return (unitVal > (previouslVal + .5f)) || (unitVal < (previouslVal - .5f));
        }
    }
}
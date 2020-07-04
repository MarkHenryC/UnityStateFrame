using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Stackable : Graspable 
	{
        public ControllerRoll controllerRollTarget;

        public enum ControllerRoll { RotatesZ, RotatesY, RotatesX };

        private Vector3 baseRotation;
        private float baseControllerZRotation;

        private const float angleEpsilon = 0.01f;

        public override void UpdateRotation(Quaternion rotation)
        {
            float controllerZRotation = rotation.eulerAngles.z - baseControllerZRotation;

            Quaternion rot = Quaternion.identity;
            float unitVal = 0f;
            switch (controllerRollTarget)
            {
                case ControllerRoll.RotatesZ:
                    rot = Quaternion.Euler(baseRotation.x, baseRotation.y, baseRotation.z + controllerZRotation);
                    if (rot.eulerAngles.z > angleEpsilon)
                        unitVal = 1f - (rot.eulerAngles.z / 360f);
                    break;
                case ControllerRoll.RotatesY:

                    rot = Quaternion.Euler(baseRotation.x, baseRotation.y - controllerZRotation, baseRotation.z);
                    if (rot.eulerAngles.y > angleEpsilon)
                        unitVal = 1f - (rot.eulerAngles.y / 360f);
                    break;
                case ControllerRoll.RotatesX:
                    rot = Quaternion.Euler(baseRotation.x + controllerZRotation, baseRotation.y, baseRotation.z);
                    if (rot.eulerAngles.x > angleEpsilon)
                        unitVal = 1f - (rot.eulerAngles.x / 360f);
                    break;
            }

            if (unitVal > 0f)
            {
                transform.rotation = rot;
            }
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);

            baseRotation = transform.rotation.eulerAngles;
            baseControllerZRotation = info.ControllerRotation.eulerAngles.z;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            baseRotation = transform.rotation.eulerAngles;
        }


    }
}
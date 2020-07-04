using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{    
	public class SqueezableTube : Pourable 
	{
        public Bender bender;

        private float zRotation; // Cache this for updating in MoveTo

        protected override void HandleZRotationVisual(float zRot)
        {
            if (zRot <= maxPourAngle)
            {
                zRotation = zRot;
                SetRotations();

                Squeeze(-zRot);
            }
        }

        public void Squeeze(float amount = 0f)
        {
            bender.Angle = amount;
        }

        public override void MoveTo(Vector3 newPos)
        {
            base.MoveTo(newPos);
            SetRotations();
        }

        /// <summary>
        /// Concatenate the LookRotation with the rot around 
        /// the z axis of the object - in that order
        /// </summary>
        private void SetRotations()
        {
            transform.rotation = Utils.ZRotateFacingController(transform.position, zRotation);
        }
    }
}
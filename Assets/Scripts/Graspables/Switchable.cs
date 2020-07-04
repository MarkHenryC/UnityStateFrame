using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace QS
{
	public class Switchable : Graspable 
	{
        public GameObject[] upImages, downImages;
        public GameObject upDownObject;
        public Vector3 localUpPosition, localDownPosition;
        public TextMeshPro label;

        public bool defaultIsUp = true;

        public Action<bool> CallOnSwitch;
        public Action<bool, Switchable> CallOnSwitchId;

        public bool IsCurrentlyUp { get { return isUp; } }

        private bool isUp;

        protected override void Awake()
        {
            base.Awake();

            Switch(defaultIsUp);
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);

            Switch(!isUp);
        }

        public void Switch(bool up)
        {
            if (upImages != null)
            {
                foreach (var upIm in upImages)
                    upIm.SetActive(up);
            }
            if (downImages != null)
            {
                foreach (var dnIm in downImages)
                    dnIm.SetActive(!up);
            }

            if (upDownObject)
                upDownObject.transform.localPosition = up ? localUpPosition : localDownPosition;

            isUp = up;

            CallOnSwitch?.Invoke(isUp);
            CallOnSwitchId?.Invoke(isUp, this);
        }

        public void SetLabel(string s)
        {
            if (label)
                label.text = s;
        }

        public override void MoveTo(Vector3 newPos)
        {
            
        }

        public override void UpdateRotation(Quaternion rotation)
        {

        }
    }
}
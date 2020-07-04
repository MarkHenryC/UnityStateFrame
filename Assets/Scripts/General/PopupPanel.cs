using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DentedPixel;

namespace QS
{
    /// <summary>
    /// Popup rotate on x
    /// </summary>
	public class PopupPanel : MonoBehaviour 
	{
        public float endAngleX = 0f;
        public Sprite image;
        public InfoPanel infoPanel;
        public Shootable shootable;
        public string label;
        public int id; // general purpose custom data
        public float popTime;
        public AudioClip popupNoise;

        private bool isActive;
        private float upTime, timeCounter;
        private Vector3 startAngle, endAngle;

		void Awake () 
		{
            startAngle = transform.localRotation.eulerAngles;
            endAngle = startAngle;
            endAngle.x = endAngleX;

            isActive = false;
            shootable.Dormant = true;
            shootable.GetComponent<SpriteRenderer>().sprite = image;
            if (label.Usable() && label != "No label") // hack!
                infoPanel.SetText(label);
            else
                infoPanel.SetText(image.name);
            gameObject.SetActive(false);
        }
		
		void Start () 
		{
			
		}

        private void Update()
        {
            if (isActive && upTime > 0f)
            {
                timeCounter += Time.deltaTime;
                if (timeCounter >= upTime)
                    PopDown();
            }
        }

        public void PopUp(float time = 0f)
        {
            upTime = time;
            isActive = true;
            shootable.Dormant = false;
            timeCounter = 0f;
            gameObject.SetActive(true);
            if (popupNoise)
                LeanAudio.playClipAt(popupNoise, transform.position).spatialBlend = 1f;
            LeanTween.rotateLocal(gameObject, endAngle, popTime)
                .setEaseOutBounce();
        }

        public void PopDown()
        {
            isActive = false;
            shootable.Dormant = true;
            LeanTween.rotateLocal(gameObject, startAngle, popTime)
                .setEaseInBounce().setOnComplete(() => { gameObject.SetActive(false); });

        }
    }
}
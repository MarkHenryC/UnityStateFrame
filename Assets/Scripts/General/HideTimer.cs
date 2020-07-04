using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace QS
{
	public class HideTimer : MonoBehaviour 
	{
        public float startHideTime;
        public float startDelayTime;
        public UnityEvent ueOnHide;

        private float timeCounter;
        private bool introDelay, timedHide;

		void Awake () 
		{
        }

        /// <summary>
        /// For connecting UnityEvents
        /// </summary>
        public void Reveal()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// For connecting UnityEvents
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (startDelayTime > 0f)
            {
                gameObject.SetActive(false);
                introDelay = true;
            }
            if (startHideTime > 0f)
                timedHide = true;
        }

        void Update() 
		{
            timeCounter += Time.deltaTime;
            if (introDelay)
            {
                if (timeCounter >= startDelayTime)
                {
                    timeCounter = 0f;
                    introDelay = false;
                    gameObject.SetActive(true);
                }
            }
            else if (timedHide)
            {
                if (timeCounter >= startHideTime)
                {
                    gameObject.SetActive(false);
                    timedHide = false;
                    if (ueOnHide != null)
                        ueOnHide.Invoke();
                }
            }
        }	
	}
}
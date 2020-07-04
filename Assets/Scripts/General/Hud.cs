using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace QS
{    
	public class Hud : MonoBehaviour 
	{
        public TextMeshProUGUI timeRemainingText;
        public TextMeshProUGUI lifeLabel, timeLeftLabel;

        public GameObject timeRemainingIndicator;
        public GameObject damageIndicator;

        public SpriteRenderer avatar;
        public GameObject burnEffect;

        private string timeFormat;
        private Material timeGauge, damageGauge;

		void Awake () 
		{
		    timeFormat = string.Format("F{0:D}", 2);
            timeGauge = timeRemainingIndicator.GetComponent<Renderer>().sharedMaterial;
            damageGauge = damageIndicator.GetComponent<Renderer>().sharedMaterial;
        }
		
		void Start () 
		{
			
		}	

        public void SetAvatar(Sprite s)
        {
            avatar.gameObject.SetActive(true);
            if (avatar.sprite != null)
                Destroy(avatar.sprite);
            avatar.sprite = s;
        }

        public void LifeIndicator(bool state)
        {
            if (state && !damageGauge)
                damageGauge = damageIndicator.GetComponent<Renderer>().sharedMaterial;

            lifeLabel.gameObject.SetActive(state);
            damageIndicator.SetActive(state);
        }

        public void TimeIndicator(bool state)
        {
            if (state && ! timeGauge)
                timeGauge = timeRemainingIndicator.GetComponent<Renderer>().sharedMaterial;

            timeRemainingText.gameObject.SetActive(state);
            timeLeftLabel.gameObject.SetActive(state);
            timeRemainingIndicator.SetActive(state);
        }

        public void SetTime(float unitVal, float seconds)
        {
            timeRemainingText.text = seconds.ToString(timeFormat);
            timeGauge.SetFloat("_Fill", unitVal);
        }

        public void SetDamage(float damage)
        {
            damageGauge.SetFloat("_Fill", damage);
        }
	}
}
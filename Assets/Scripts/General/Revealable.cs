using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    /// <summary>
    /// A way of automating visibility
    /// of in-scene objects. An alternative
    /// to Arrayable when the objects are
    /// already extant
    /// </summary>
	public class Revealable : MonoBehaviour 
	{
        public Transform[] items;
        public bool startVisible;
        
        public AudioClip soundEffect;

        private AudioSource audioSource;
        private Coroutine cycler;
        private Action OnComplete;

        void Awake () 
		{
            Debug.Log("Awake() " + this.name);
            SetToDefault();
        }
		
		void Start () 
		{
			
		}

        /// <summary>
        /// Editor version
        /// </summary>
        public void Reveal()
        {
            for (int i = 0; i < items.Length; i++)
                items[i].gameObject.SetActive(!startVisible);
        }

        public void Reveal(float delay, Action onComplete = null)
        {
            StartCoroutine(CrReveal(delay, onComplete));
        }

        public void Cycle(float interval)
        {
            cycler = StartCoroutine(CrCycle(interval));
        }

        public void StopCycle()
        {
            if (cycler != null)
                StopCoroutine(cycler);
            cycler = null;
        }

        public void SetToDefault()
        {
            for (int i = 0; i < items.Length; i++)
                items[i].gameObject.SetActive(startVisible);
        }

        private IEnumerator CrReveal(float delay, Action onComplete)
        {
            var crDelay = new WaitForSeconds(delay);

            for (int i = 0; i < items.Length; i++)
            {
                items[i].gameObject.SetActive(!startVisible);
                yield return crDelay;
            }

            onComplete?.Invoke();
        }

        private IEnumerator CrCycle(float delay)
        {
            var crDelay = new WaitForSeconds(delay);

            for (; ; )
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].gameObject.SetActive(!startVisible);
                    yield return crDelay;
                }

                SetToDefault();
                yield return crDelay;
            }
        }

        private void OnDestroy()
        {
            SetToDefault();
        }
    }
}
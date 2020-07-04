using UnityEngine;
using System;

namespace QS
{
    public class ActivateFor : MonoBehaviour
    {
        public float duration = 1f;
        public bool startActivated;

        private void Awake()
        {
            gameObject.SetActive(startActivated);
        }

        public void Activate(Action a)
        {
            gameObject.SetActive(true);
            LeanTween.delayedCall(duration, () => 
            {
                gameObject.SetActive(false);
                a?.Invoke();
            });
        }

    }
}
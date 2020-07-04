using UnityEngine;

namespace QS
{
    /// <summary>
    /// Narrow purpose: flash a prompt
    /// and show a label
    /// </summary>
	public class FlashAndLabel : MonoBehaviour
    {
        public GameObject flasher, label;
        public float onTime = .75f, offTime = .25f;

        private bool active;
        private float offCounter, onCounter;

        public bool Active
        {
            get => active;

            set
            {
                if (active != value)
                {
                    active = value;

                    flasher.SetMode(active);
                    label.SetMode(active);

                    offCounter = 0f;
                    if (active)
                        onCounter = onTime;
                    else
                        onCounter = 0f;
                }
            }
        }

        private void Awake()
        {
            if (!label)
                label = gameObject;
            active = gameObject.activeSelf;

        }

        private void Start()
        {

        }

        private void Update()
        {
            if (Active)
            {
                if (onCounter > 0f)
                {
                    onCounter -= Time.deltaTime;
                    if (onCounter < 0f)
                    {
                        offCounter = offTime;
                        flasher.SetMode(false);
                    }
                }
                else if (offCounter > 0f)
                {
                    offCounter -= Time.deltaTime;
                    if (offCounter < 0f)
                    {
                        onCounter = onTime;
                        flasher.SetMode(true);
                    }
                }
            }
        }
    }
}
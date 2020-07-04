using UnityEngine;
using UnityEngine.Events;

namespace QS
{
    [RequireComponent(typeof(AudioSource))]
    public class MasterAnimator : MonoBehaviour
    {
        public GameObject activateOnArrived;
        public AudioClip leftFoot, rightFoot, skid, roll;
        public string initialTrigger;
        public UnityEvent OnFinishedLooking;
        public Animator modelAnimator;

        private int walkSpeedId;
        private AudioSource footNoises;

        private void Awake()
        {
            walkSpeedId = Animator.StringToHash("WalkSpeed");
            footNoises = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (initialTrigger.Usable())
                modelAnimator.SetTrigger(initialTrigger);
            else if (activateOnArrived)
                activateOnArrived.SetActive(true);
        }

        public void Clap()
        {

        }

        public void LeftFoot()
        {
            footNoises.PlayOneShot(leftFoot);
        }

        public void RightFoot()
        {
            footNoises.PlayOneShot(rightFoot);
        }

        public void Skid()
        {
            footNoises.PlayOneShot(skid);
        }

        public void Roll()
        {
            footNoises.PlayOneShot(roll);
        }

        public void FinishedLooking()
        {
            if (OnFinishedLooking != null)
                OnFinishedLooking.Invoke();
        }

        public void Arrived()
        {
            Debug.Log("Arrived");

            if (activateOnArrived)
                activateOnArrived.SetActive(true);
        }

        private void _OnAnimatorMove()
        {
            if (modelAnimator)
            {
                Vector3 newPosition = transform.position;
                AnimatorStateInfo stateInfo = modelAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsTag("Walking"))
                {
                    newPosition.z += modelAnimator.GetFloat(walkSpeedId) * Time.deltaTime;
                    transform.position = newPosition;
                }
            }
        }

    }
}
using UnityEngine;
using UnityEngine.Events;

namespace QS
{
    public class ChefAnimator : MonoBehaviour
    {
        public GameObject activateOnArrived;
        public AudioClip leftFoot, rightFoot, skid, roll;
        public string initialTrigger;
        public UnityEvent OnFinishedLooking;

        private Animator anim;
        private int walkSpeedId;
        private AudioSource footNoises;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            walkSpeedId = Animator.StringToHash("WalkSpeed");
            footNoises = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (initialTrigger.Usable())
                anim.SetTrigger(initialTrigger);
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
            if (anim)
            {
                Vector3 newPosition = transform.position;
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsTag("Walking"))
                {
                    newPosition.z += anim.GetFloat(walkSpeedId) * Time.deltaTime;
                    transform.position = newPosition;
                }
            }
        }

    }
}
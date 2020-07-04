using System;
using UnityEngine;
using UnityEngine.AI;

namespace QS
{
    public class NpcController : MonoBehaviour
    {
        public bool autoPlay;
        public Transform[] waypoints;
        public float loopTime;
        public string defaultAnimTrigger;
        public string restingAnim;
        public bool once; // default is false for backward compatibility
        public LipSync lipSync;

        private Animator animator;
        private BlendShapeManager blendShapeManager;
        private float speed; // metres per second
        private bool walking, navigating;
        private Transform startTransform;
        private LTDescr playingAnim;
        private Action callOnTalkComplete;
        private AudioSource audioSource;
        private Hazard hazard;
        private NavMeshAgent navAgent;
        private Vector3 currentNavTarget;
        private Action<NpcController> onReachedTarget;
        private TriggerNotify triggerNotify;
        private Action<NpcController, Collider> triggerEnter, triggerExit;
        private Action<NpcController, Collision> collisionEnter, collisionExit;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
            blendShapeManager = GetComponent<BlendShapeManager>();
            audioSource = GetComponent<AudioSource>();
            hazard = GetComponent<Hazard>();
            navAgent = GetComponent<NavMeshAgent>();
            triggerNotify = GetComponent<TriggerNotify>();

            startTransform = transform;
        }

        private void OnEnable()
        {
            if (autoPlay)
                StartMovement();
        }

        private void Update()
        {
            if (walking)
                transform.localPosition += (transform.forward * speed * Time.deltaTime);

            if (callOnTalkComplete != null)
            {
                if (!lipSync.IsPlaying)
                {
                    callOnTalkComplete();
                    callOnTalkComplete = null;
                }
            }

            if (navAgent && navigating && onReachedTarget != null)
            {
                if (Vector3.SqrMagnitude(currentNavTarget - transform.position) <= (navAgent.radius * navAgent.radius))
                //if (navAgent.remainingDistance <= navAgent.radius) <- returns 0 sometimes!
                {
                    var swapAction = onReachedTarget;
                    Debug.LogFormat("{0} is now {1} from target", gameObject.name, navAgent.remainingDistance);

                    onReachedTarget = null;
                    swapAction(this);                    
                }
            }
        }

        public void Show(bool show = true)
        {
            gameObject.SetMode(show);
        }

        public void SetTriggers(Action<NpcController, Collider> onTriggerEnter, Action<NpcController, Collider> onTriggerExit)
        {
            triggerEnter = onTriggerEnter;
            triggerExit = onTriggerExit;
        }

        public void SetCollisions(Action<NpcController, Collision> onCollisionEnter, Action<NpcController, Collision> onCollisionExit)
        {
            collisionEnter = onCollisionEnter;
            collisionExit = onCollisionExit;
        }

        public void StartMovement()
        {
            if (once)
                PlayOnce(waypoints, loopTime);
            else
                LoopPath(waypoints, loopTime);

            if (defaultAnimTrigger.Usable())
                SetAnimation(defaultAnimTrigger);
        }

        public void SetTransform(Transform pos)
        {
            transform.Set(pos);
        }

        public void SetAnimation(string name)
        {
            if (animator && name.Usable())
            {
                //Debug.Log("Triggering animation: " + name);
                animator.SetTrigger(name);
            }
        }

        public bool IsCurrentState(string name, int layerIx = 0)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
        }

        public void PlayHazardActiveSound()
        {
            if (hazard)
                hazard.PlayActiveSound();
        }

        public void SetExpression(string name, float morphTime = 1f, float holdTime = 2f)
        {
            if (blendShapeManager)
                blendShapeManager.SetExpression(name, morphTime, holdTime);
        }

        public void MoveForward(float walkSpeed)
        {
            speed = walkSpeed;
            walking = (speed != 0);
        }

        public void NavTo(Vector3 pos, Action<NpcController> onReached = null)
        {
            if (navAgent)
            {
                currentNavTarget = pos;
                onReachedTarget = onReached;

                navAgent.SetDestination(currentNavTarget);
                navigating = true;
            }
        }

        public void ResetNav()
        {
            if (navAgent)
            {
                navigating = false;
                if (navAgent.isOnNavMesh)
                    navAgent.ResetPath();
            }
        }

        public void SetReachedTargetNotify(Action<NpcController> a)
        {
            onReachedTarget = a;
        }

        public void Talk(AudioClip voice, Action callOnComplete = null)
        {
            if (lipSync)
            {
                lipSync.SetAudioClip(voice);
                lipSync.Play();
                callOnTalkComplete = callOnComplete;
            }
        }

        /// <summary>
        /// Used for general NPC movement
        /// </summary>
        /// <param name="waypoints"></param>
        /// <param name="timeToComplete"></param>
        /// <param name="loop"></param>
        public void MoveAlongPath(Transform[] waypoints, float timeToComplete, bool loop)
        {
            Vector3[] animPath = Utils.SimpleSplinePath(waypoints, loop);

            playingAnim = LeanTween.moveSpline(gameObject, animPath, timeToComplete)
                .setOrientToPath(true)
                .setLoopClamp();
        }

        public void MoveAlongPathThen(Transform[] waypoints, float timeToComplete, string restAnim)
        {
            Vector3[] animPath = Utils.SimpleSplinePath(waypoints, false);

            if (playingAnim != null)
                return;

            playingAnim = LeanTween.moveSpline(gameObject, animPath, timeToComplete)
                .setOrientToPath(true).setLoopType(LeanTweenType.once).setOnComplete(
                () =>
                {
                    SetAnimation(restAnim);
                    playingAnim = null;
                }
                );
        }

        public void ResetTransform()
        {
            if (playingAnim != null)
            {
                LeanTween.cancel(playingAnim.id);
                playingAnim = null;
            }
            SetTransform(startTransform);
        }

        public void SetAtStart()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                transform.Set(waypoints[0]);
                if (waypoints.Length > 1)
                    transform.LookAt(waypoints[1].position);
            }
        }

        public void PlayDefault()
        {
            LoopPath(waypoints, loopTime);
            if (defaultAnimTrigger.Usable())
                SetAnimation(defaultAnimTrigger);
        }

        public void Stop(string restAnim = null)
        {
            if (playingAnim != null)
            {
                LeanTween.cancel(playingAnim.id);
                playingAnim = null;
            }
            if (restAnim.Usable())
                SetAnimation(restAnim);
        }

        /// <summary>
        /// Used in autoplay mode
        /// </summary>
        /// <param name="waypoints"></param>
        /// <param name="timeToComplete"></param>
        private void LoopPath(Transform[] waypoints, float timeToComplete)
        {
            Vector3[] animPath = Utils.SimpleSplinePath(waypoints, true);

            playingAnim = LeanTween.moveSpline(gameObject, animPath, timeToComplete)
                .setOrientToPath(true).setLoopType(LeanTweenType.linear);
        }

        private void PlayOnce(Transform[] waypoints, float timeToComplete)
        {
            if (playingAnim != null)
                return;

            Vector3[] animPath = Utils.SimpleSplinePath(waypoints, true); // Why did I set loop flag?

            playingAnim = LeanTween.moveSpline(gameObject, animPath, timeToComplete)
                .setOrientToPath(true).setLoopType(LeanTweenType.once).setOnComplete(
                () =>
                    {
                        SetAnimation(restingAnim);
                        playingAnim = null;
                    }
                );
        }

        private void OnTriggerEnter(Collider other)
        {
            triggerEnter?.Invoke(this, other);
        }

        private void OnTriggerExit(Collider other)
        {
            triggerExit?.Invoke(this, other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            collisionEnter?.Invoke(this, collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            collisionExit?.Invoke(this, collision);
        }
    }
}
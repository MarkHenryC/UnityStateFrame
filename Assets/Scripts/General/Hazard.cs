using System;
using UnityEngine;

namespace QS
{
    /// <summary>
    /// Intended for positional hazards, such as
    /// flare-ups in the kitchen. Also supports
    /// regiseting a "bump" action, such as when
    /// attaching a hazard to an arbitrary object
    /// (such as the chef model, which has a
    /// trigger collider attached). Also has, as
    /// a bit of a late hack, registration of
    /// any other arbitrary object that intersects
    /// this hazard. The default behaviour is 
    /// when player enters hazard area or bumps
    /// hazard object, but we may also need to 
    /// know if any other object intersects.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        public float effectStrength;
        public GameObject activeVisual;
        public bool selfOscillating;
        public float offTime, onTime, startTime;
        public AudioLooper audioLooper;
        public Action<Hazard> callOnEnter, callOnExit;
        public Action<Hazard, float> callOnDamage;
        public Action<Hazard> callOnBump;
        public bool bumpDamage;
        public AudioSource collisionSoundPlayer;
        public AudioClip collisionSound, activeSound;
        public float bumpRetriggerDelay;
        public bool positionalImpactSound; // For backward compat
        public string damageTriggerName;
        public bool pauseWhenHitPlayer;

        public bool Active { set; get; }
        public bool IsHarmful { set; get; }
        public float Damage => cumulativeHarm;

        public bool WithinBumpRange { get; private set; }

        private bool isWithin;
        private float cumulativeHarm;
        private float cumulativeOff, cumulativeOn;
        private bool started;
        private float timeSinceLastDamage;
        private Animator animator;

        private const string PLAYER = "Player";

        public void Clear()
        {
            isWithin = false;
            cumulativeHarm = 0f;
            started = false;
        }

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (activeVisual)
                activeVisual.SetActive(false);
        }

        private void Update()
        {
            if (isWithin && IsHarmful)
            {
                float currentDamage = effectStrength * Time.deltaTime;
                cumulativeHarm += currentDamage;
                if (callOnDamage != null)
                {
                    Debug.Log("Hazard: CallOnDamage");

                    callOnDamage(this, currentDamage);
                }
            }

            if (selfOscillating)
            {
                if (!started)
                {
                    cumulativeOff += Time.deltaTime;
                    if (cumulativeOff >= startTime)
                    {
                        started = true;
                        SetActiveHarm(true);
                        cumulativeOff = 0f;
                    }
                }
                else
                {
                    if (IsHarmful)
                    {
                        cumulativeOn += Time.deltaTime;
                        if (cumulativeOn >= onTime)
                        {
                            SetActiveHarm(false);
                            cumulativeOn = 0f;
                        }
                    }
                    else
                    {
                        cumulativeOff += Time.deltaTime;
                        if (cumulativeOff >= offTime)
                        {
                            SetActiveHarm(true);
                            cumulativeOff = 0f;
                        }
                    }
                }
            }
        }

        public virtual void SetActiveHarm(bool harm)
        {
            IsHarmful = harm;
            if (activeVisual)
                activeVisual.SetActive(harm);
            if (audioLooper)
                audioLooper.Sync(harm);
            if (activeSound && harm)
                LeanAudio.playClipAt(activeSound, transform.position).spatialBlend = 1f;
        }

        public virtual void PlayActiveSound()
        {
            if (activeSound)
                LeanAudio.playClipAt(activeSound, transform.position).spatialBlend = 1f;
        }
        // Added check for enabled so we don't do
        // collision noises outside of challenge

        private void OnCollisionEnter(Collision collision)
        {
            if (enabled)
                HandleContact(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (enabled)
                EndContact(collision.collider);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (enabled)
                HandleContact(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (enabled)
                EndContact(other);
        }

        /// <summary>
        /// Added some debug stuff as we're getting different
        /// behaviour on Anrdoid (Oculus Go). Time mute
        /// period is not happening
        /// </summary>
        /// <param name="other"></param>
        private void HandleContact(Component other)
        {
            if (other.name.StartsWith(PLAYER))
            {
                WithinBumpRange = true;

                if (bumpDamage)
                {
                    if (bumpRetriggerDelay != 0f)
                    {
                        if (Time.time - timeSinceLastDamage < bumpRetriggerDelay)
                        {
                            Debug.Log("Hazard: Bumped during mute period");

                            return;
                        }
                        else
                        {
                            timeSinceLastDamage = Time.time;
                        }
                    }

                    if (collisionSound)
                    {
                        if (collisionSoundPlayer)
                            collisionSoundPlayer.PlayOneShot(collisionSound);
                        else
                        {
                            if (positionalImpactSound)
                                LeanAudio.play(collisionSound, transform.position);
                            else
                                LeanAudio.play(collisionSound);
                        }
                    }

                    if (animator)
                        animator.SetTrigger(damageTriggerName);

                    cumulativeHarm += effectStrength;

                    if (callOnBump != null)
                    {
                        Debug.Log("Hazard: CallOnBump");

                        callOnBump(this);
                    }

                    if (callOnDamage != null)
                    {
                        Debug.Log("Hazard - bump: CallOnDamage");

                        callOnDamage(this, effectStrength);
                    }

                }
                else
                    isWithin = true;
            }
            else
            {
                Placeable placeable = other.GetComponent<Placeable>();
                if (placeable)
                {
                    Debug.Log("COLLISION with placeable " + other.name);

                    if (placeable.CallOnEnterHazard != null)
                        placeable.CallOnEnterHazard(placeable, this);
                }
            }
        }

        private void EndContact(Component other)
        {
            if (other.name.StartsWith(PLAYER))
            {
                isWithin = false;
                WithinBumpRange = false;
            }
        }
    }
}
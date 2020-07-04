using System;
using UnityEngine;

namespace QS
{
    public class Pourable : Placeable, IUnitValueProvider
    {
        public Action<Pourable, float> Pouring;
        public Action<Pourable, float> PouringComplete;
        public Action<Pourable> OnEmpty;
        [Tooltip("Currently we're assuming a left-handed pour with the forward vector of the object toward the player")]
        public float minPourAngle = 45f, maxPourAngle = 180f;
        [Tooltip("This is a UnitInterval, not the domain metric for the given substance")]
        public float pourRateUnitsPerSecond = 0.2f;
        [Tooltip("Whatever the measuring form is")]
        public string metric = "Millilitres";
        [Tooltip("The coefficient by which to multiply pourAccumulator. Basically the size of the bottle in domain units - which corresponds to 1.0.")]
        public float maxUnits;
        [Tooltip("Attached animation for when pouring.")]
        public GameObject pouringAnim;
        [Tooltip("Optional player for sfx.")]
        public AudioSource audioSource;
        [Tooltip("A looped clip for when pouring.")]
        public AudioClip pourNoise;
        [Tooltip("The start point for pouring.")]
        public GameObject pouringTip;
        [Tooltip("Make sure we're over target.")]
        public LayerMask pourTarget;
        public ActionFloatProvider newValueAction;
        [Tooltip("If this is true we'll always call pour callback even if not over target (such as for tutorial).")]
        public bool pourAnywhere;
        protected bool isSelected;
        protected bool empty;
        protected float pourRange;
        protected float pourAccumulator, targetedPourTotal;
        protected string tooltipText;
        protected bool isPouring;
        protected Vector3 baseRotation;
        protected Vector3 baseRotationController;

        protected override void Awake()
        {
            base.Awake();

            pourRange = maxPourAngle - minPourAngle;
            Debug.Assert(pourRange > 0, "Invalid pour range");

            tooltipText = string.Format("{0} {1} container of {1}", maxUnits, metric, gameObject.name);
            pouringAnim.SetMode(false);
            if (!audioSource)
                audioSource = GetComponent<AudioSource>();
            if (audioSource)
            {
                audioSource.clip = pourNoise;
                audioSource.loop = true;
            }
        }

        public void AddListener(Action<IUnitValueProvider, float> a)
        {
            if (newValueAction)
                newValueAction.AddResponder(a);
        }

        public void RemoveListener(Action<IUnitValueProvider, float> a)
        {
            if (newValueAction)
                newValueAction.RemoveResponder(a);
        }

        /// <summary>
        /// UnitValue of what was poured
        /// </summary>
        public float PourAccumulator
        {
            get { return targetedPourTotal; }
        }

        /// <summary>
        /// Convert pour units to amounts. PourAccumulator
        /// is just the unitValue.
        /// </summary>
        public float ActualAmount
        {
            get { return targetedPourTotal * maxUnits; }
        }

        public override void UpdateRotation(Quaternion rotation)
        {
            float zRot = rotation.eulerAngles.z;
            Vector3 axis = transform.position - ControllerInput.Instance.ControllerPosition;

            // This can be handled according to the subclass. Standard
            // behaviour is to turn the object to the left along z
            HandleZRotationVisual(zRot);

            if (isSelected)
            {
                if (Pouring != null || newValueAction != null && !empty)
                {
                    bool pour = false;
                    if (zRot <= maxPourAngle && zRot >= minPourAngle)
                    {
                        float pourSpeed = zRot / pourRange;
                        pourAccumulator += pourSpeed * pourRateUnitsPerSecond * Time.deltaTime;
                        Debug.Log(pourAccumulator);
                        if (pourAccumulator >= 1f)
                        {
                            empty = true;
                            if (OnEmpty != null)
                                OnEmpty(this);
                            SetClickDownText("Empty");
                        }
                        else if (inProximity || pourAnywhere)
                        {
                            targetedPourTotal += pourSpeed * pourRateUnitsPerSecond * Time.deltaTime;
                            targetedPourTotal = Mathf.Clamp(targetedPourTotal, 0f, 1f);

                            if (Pouring != null)
                                Pouring(this, targetedPourTotal);
                            if (newValueAction)
                                newValueAction.Invoke(this, targetedPourTotal);

                            pour = true;
                        }
                        
                    }

                    SetPour(pour);
                }
            }

            baseRotationController = rotation.eulerAngles;
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);

            Utils.FaceCamera(transform);

            baseRotation = transform.rotation.eulerAngles;
            baseRotationController = info.ControllerRotation.eulerAngles;

            Debug.LogFormat("Base rotation pourable: {0}. Base rotation controller: {1}", baseRotation, baseRotationController);

            isSelected = true;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            if (PouringComplete != null)
                PouringComplete(this, targetedPourTotal);

            baseRotation = transform.rotation.eulerAngles;

            isSelected = false;
            SetPour(false);
        }

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            SetTooltip(tooltipText);
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            SetTooltip();
        }

        public override void HandleHorizontalSwipe(float amount)
        {
            // Nothing to do here
        }

        protected virtual void HandleZRotationVisual(float zRot)
        {
            // For a pourable, the axis from controller to target, so rotation
            // is in line Player's view
            Vector3 axis = transform.position - ControllerInput.Instance.ControllerPosition;
            transform.rotation = Quaternion.AngleAxis(zRot, axis);
        }

        protected void SetPour(bool pour)
        {
            if (pour && !empty)
            {
                if (!isPouring)
                {
                    if (pouringAnim)
                        pouringAnim.SetMode(true);
                    if (audioSource && pourNoise)
                        audioSource.Play();
                    isPouring = true;
                }
            }
            else
            {
                if (isPouring)
                {
                    if (pouringAnim)
                        pouringAnim.SetMode(false);
                    if (audioSource && pourNoise)
                        audioSource.Stop();
                    isPouring = false;
                }
            }
        }
    }
}
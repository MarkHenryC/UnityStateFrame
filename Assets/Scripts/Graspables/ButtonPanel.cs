using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace QS
{
    public class ButtonPanel : Graspable
    {
        public UnityEvent clickUpEvent;
        public Action trigger;

        public bool lookAtPlayer = true;
        public TextMeshPro data;
        public float zMove, moveTime;
        public bool highlight = true;
        public bool inactive;
        public GameObject timeRemainingIndicator;

        protected Vector3 defaultPos, rolloverPos;
        protected Material highlighter;

        protected const float pulseRate = 0.5f;
        protected float pulseCounter = 0f;
        protected bool pulseOn;
        protected Material timeGauge;
        protected float timeSpan, timeLeft;
        protected bool timedExit;
        protected Action timedAction;

        protected override void Awake()
        {
            base.Awake();
            if (timeRemainingIndicator)
                timeGauge = timeRemainingIndicator.GetComponent<Renderer>().sharedMaterial;
            defaultPos = transform.position;
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in renderers)
            {
                if (rend.name == "ButtonPrefab")
                {
                    highlighter = rend.sharedMaterial;
                    break;
                }
            }
            if (inactive)
                Dormant = true;
            if (!data)
                data = GetComponentInChildren<TextMeshPro>();
        }

        public void SetText(string text)
        {
            data.text = text;
        }

        public void Activate(string text, Action<Graspable> clickAction)
        {
            if (text.Usable())
                data.text = text;
            ReleaseTrigger();
            clickUpAction = clickAction;
            Show(true);
        }

        public void Deactivate()
        {
            clickUpAction = null;
        }

        public void SetTrigger(string text, Action a)
        {
            trigger = a;
            if (text.Usable())
                data.text = text;
            Deactivate();
            Show(true);
        }

        public void ReleaseTrigger()
        {
            trigger = null;
        }

        protected virtual void HandleEnter()
        {
            rolloverPos = defaultPos - transform.forward * zMove;
            LeanTween.move(gameObject, rolloverPos, moveTime);
        }

        protected virtual void HandleExit()
        {
            LeanTween.move(gameObject, defaultPos, moveTime);
        }

        /// <summary>
        /// For connecting UnityEvents
        /// </summary>
        public void Reveal()
        {
            Show(true);
        }

        /// <summary>
        /// For connecting UnityEvents
        /// </summary>
        public void Hide()
        {
            Show(false);
        }

        public override bool IsButton()
        {
            return true;
        }

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            if (zMove != 0)
                HandleEnter();
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            if (zMove != 0)
                HandleExit();
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            if (pointerWithin && clickUpEvent != null)
                clickUpEvent.Invoke();
            if (trigger != null)
                trigger();

            timedAction = null;
        }

        public override void HandleHorizontalSwipe(float amount)
        {

        }

        public override void Rotate(float ex, float ey, float ez)
        {

        }

        public override void MoveTo(Vector3 newPos)
        {

        }

        public void ActivateWithTimeout(string text, Action<Graspable> clickAction, Action timedOut, float timeoutSeconds)
        {
            if (text.Usable())
                data.text = text;
            ReleaseTrigger();
            clickUpAction = clickAction;
            Show(true);
            if (timedOut != null)            
                DoOnTimeout(timedOut, timeoutSeconds);            
        }

        public void DoOnTimeout(Action a, float seconds)
        {
            timedExit = true;
            timedAction = a;
            timeSpan = seconds;
            timeLeft = timeSpan;
            float unitVal = timeLeft / timeSpan;
            if (timeGauge)
                timeGauge.SetFloat("_Fill", unitVal);
        }

        protected override void Update()
        {
            base.Update();
            if (lookAtPlayer)
                Utils.FaceCamera(transform);

            if (timedExit && timedAction != null)
            {
                timeLeft -= Time.deltaTime;
                float unitVal = timeLeft / timeSpan;
                if (timeGauge)
                    timeGauge.SetFloat("_Fill", unitVal);
                if (timeLeft <= 0f)
                {
                    Deactivate();
                    if (timedAction != null)
                        timedAction();
                    Show(false);
                }
            }
        }
    }
}

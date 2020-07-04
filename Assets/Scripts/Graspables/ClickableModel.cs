using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class ClickableModel : ButtonPanel 
	{
        public GameObject selectedMarker, deselectedMarker;
        public int sortingOrder;
        public int AllocatedIndex { get; set; }
        public bool Selected { get; private set; }
        public Action<ClickableModel> OnClick;
        public Animator animator;
        public string rolloverTrigger, idleTrigger, clickUpTrigger; // Animation transitions
        public GameObject highlightVisual; // The shared visual for highlighting selection
        public float yOffsetHighlight;
        
        public float FloatStore { get; set; }

        private Vector3 upY;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<Animator>();
            upY = new Vector3(0, yOffsetHighlight, 0);
        }

        public void Select(bool select)
        {
            if (selectedMarker)
                selectedMarker.SetActive(select);
            if (deselectedMarker)
                deselectedMarker.SetActive(!select);
            Selected = select;
        }

        public override void Clear()
        {
            base.Clear();

            Selected = false;

            if (selectedMarker)
                selectedMarker.SetActive(false);
            if (deselectedMarker)
                deselectedMarker.SetActive(false);
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);
            if (animator)
                animator.SetTrigger(clickUpTrigger);

            if (OnClick != null)
                OnClick(this);
        }

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            if (animator)
                animator.SetTrigger(rolloverTrigger);
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            if (animator)
                animator.SetTrigger(idleTrigger);
        }

        public override void TransformToHome(bool local = false)
        {
            if (Application.isPlaying)
            {
                if (local)
                {
                    LeanTween.moveLocal(gameObject, homePosition, 1f).setEaseInOutQuad();
                    LeanTween.rotateLocal(gameObject, homeRotation.eulerAngles, 1f);
                }
                else
                {
                    LeanTween.move(gameObject, homePosition, 1f).setEaseInOutQuad();
                    LeanTween.rotate(gameObject, homeRotation.eulerAngles, 1f);
                }
            }
            else
            {
                if (local)
                {
                    transform.localPosition = homePosition;
                    transform.localRotation = homeRotation;
                }
                else
                {
                    transform.position = homePosition;
                    transform.rotation = homeRotation;
                }
            }
        }

        public override void TransformToDest(bool local = false)
        {
            if (Application.isPlaying)
            {
                if (local)
                {
                    LeanTween.moveLocal(gameObject, destPosition, 1f).setEaseInOutQuad();
                    LeanTween.rotateLocal(gameObject, destRotation.eulerAngles, 1f);
                }
                else
                {
                    LeanTween.move(gameObject, destPosition, 1f).setEaseInOutQuad();
                    LeanTween.rotate(gameObject, destRotation.eulerAngles, 1f);
                }
            }
            else
            {
                if (local)
                {
                    transform.localPosition = destPosition;
                    transform.localRotation = destRotation;
                }
                else
                {
                    transform.position = destPosition;
                    transform.rotation = destRotation;
                }
            }
        }

        /// <summary>
        /// This should be in a subclass but it'll do for now
        /// </summary>
        public void Clap()
        {
            if (highlightVisual)
            {
                highlightVisual.SetActive(true);
                highlightVisual.transform.position = transform.position + upY;
            }

            //Debug.Log("Clap");
        }
    }
}
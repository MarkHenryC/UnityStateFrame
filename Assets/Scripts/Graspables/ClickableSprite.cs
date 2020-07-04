using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using DentedPixel;

namespace QS
{
    /// <summary>
    /// Forms part of a ClickableSpriteGroup.
    /// activity
    /// </summary>
    public class ClickableSprite : ButtonPanel
    {
        public GameObject selectedMarker, deselectedMarker;
        public int sortingOrder;
        public int AllocatedIndex { get; set; }
        public bool Selected { get; private set; }
        public GameObject sourceSwap, destSwap;

        public Action<ClickableSprite> OnClick;        

        protected override void Awake()
        {
            base.Awake();
            GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
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
            if (OnClick != null)
                OnClick(this);
        }

        /// <summary>
        /// Called at runtime or in editor
        /// When destswap is supplied, the source
        /// Sprite rendered is switched off and
        /// the destswap gameobject is activated
        /// at the destination position. This allows 
        /// for more complex sprites, such as when
        /// shoes need to have a layer behind and
        /// in front of the foot (like thongs)
        /// </summary>
        /// <param name="local"></param>
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
                if (destSwap)
                    LeanTween.delayedCall(1f, () =>
                    {
                        destSwap.SetActive(false);
                        if (sourceSwap)
                            sourceSwap.SetActive(true);
                        else
                            GetComponent<SpriteRenderer>().enabled = true;
                    });

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
                if (destSwap)
                {
                    destSwap.SetActive(false);
                    if (sourceSwap)
                        sourceSwap.SetActive(true);
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
                if (destSwap)
                    LeanTween.delayedCall(1f, () => 
                    {
                        destSwap.SetActive(true);
                        if (sourceSwap)
                            sourceSwap.SetActive(false);
                        else
                            GetComponent<SpriteRenderer>().enabled = false;
                    });
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
    }
}
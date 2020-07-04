using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace QS
{
    public class Receivable : MonoBehaviour
    {
        public Color proximityColor = Color.green;
        [Tooltip("Show when in proximity (trigger)")]
        public InfoPanel infoText;
        [Tooltip("Show when collides")]
        public InfoPanel colliderText;
        [Tooltip("If fixed, does not track player. Default negative for backward compat.")]
        public bool fixedLabel;
        [Tooltip("Optional. May give a hint as to where the object will be placed")]
        public GameObject visualCue;
        [Tooltip("Optional. Visual cues can be explicitly shown by calling public method or triggered automatically when pointer is in range")]
        public bool autoShowVisualCue;
        [Tooltip("Optional. Allow for storing a preferred destination position")]
        public Placeable overrideDestination;
        [Tooltip("Optional. The Placeable we're storing")]
        public Placeable contained;
        [Tooltip("Optional. A late addition. Setting as trigger breaks physics in some dual-purpose objects")]
        public bool overrideAsTrigger;
        public bool Occupied { get; set; }

        public Action<Placeable, Receivable> callOnCollide;

        protected Placeable placeable; // Cache it
        protected Collider boxCollider;

        public virtual void SetHighlight(bool set)
        {
            if (!set && infoText)
                infoText.gameObject.SetActive(false);
        }

        protected virtual void Awake()
        {
            if (visualCue)
                visualCue.SetActive(false);
            boxCollider = GetComponent<BoxCollider>();
            if (!boxCollider)
                boxCollider = GetComponent<CapsuleCollider>();
            if (boxCollider && overrideAsTrigger)
                boxCollider.isTrigger = true;
        }

        protected virtual void Start()
        {
        }

        public virtual void ShowVisualCue(bool show = true)
        {
            if (visualCue)
                visualCue.SetActive(show);
        }

        public virtual void SetTriggerInfo(string info)
        {
            if (infoText)
            {
                infoText.gameObject.SetActive(true);
                if (info.Usable())
                    infoText.SetText(info);
                if (!fixedLabel)
                    Utils.FaceCamera(infoText.transform);
            }
        }

        public virtual void SetColliderInfo(string info)
        {
            if (colliderText)
            {
                colliderText.gameObject.SetActive(true);
                if (info.Usable())
                    colliderText.SetText(info);
                if (!fixedLabel)
                    Utils.FaceCamera(colliderText.transform);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (Occupied)
                return;

            placeable = other.GetComponent<Placeable>();
            if (placeable)
                placeable.NotifyEnterProximity(this);
            if (autoShowVisualCue)
                ShowVisualCue();
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (Occupied)
                return;

            if (placeable)
            {
                placeable.NotifyExitProximity(this);
                placeable = null;
                if (visualCue)
                    visualCue.SetActive(false);
            }
            if (infoText)
                infoText.gameObject.SetActive(false);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (Occupied)
                return;

            Debug.Log("Collision with " + collision.gameObject.name);

            var temp_placeable = collision.gameObject.GetComponent<Placeable>();
            if (temp_placeable)
            {
                temp_placeable.NotifyEnterCollision(this);
                if (callOnCollide != null)
                    callOnCollide(temp_placeable, this);
            }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (Occupied)
                return;

            var temp_placeable = collision.gameObject.GetComponent<Placeable>();
            if (temp_placeable)
                temp_placeable.NotifyExitCollision(this);
            if (colliderText)
                colliderText.gameObject.SetActive(false);
        }

    }
}

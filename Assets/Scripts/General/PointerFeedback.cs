using UnityEngine;
using System;

namespace QS
{
    public abstract class PointerFeedback : MonoBehaviour
    {
        [Tooltip("Optional: select an object for a 3D pointer")]
        public GameObject reticle;
        [Tooltip("Select a base scale for the reticle. Reticle is rescaled according to distance from camera")]
        public float reticleScale = 0.5f;
        [Tooltip("Optional: drop in a model to represent the controller")]
        public Transform controllerRepresentation;

        protected Vector3 pointerPos, pointerTarget;
        protected Quaternion pointerRot;
        protected bool gotController;
        protected Camera mainCam;
        protected bool isActive;
        protected GameObject cacheReticle;
        protected float cacheReticleScale;
        protected Action reticleHandler;

        public virtual void Awake()
        {
            mainCam = Camera.main;
            isActive = false;
            cacheReticle = reticle;
            cacheReticleScale = reticleScale;
            Activate(isActive);
        }

        /// <summary>
        /// Only intended for single temp push,
        /// rather than stack, so cach is done in 
        /// Awake()
        /// </summary>
        /// <param name="tempReticle"></param>
        /// <param name="tempScale"></param>
        public virtual void OverrideReticle(GameObject tempReticle, float tempScale = 1f)
        {
            reticle.SetActive(false);
            reticle = tempReticle;
            reticle.SetActive(true);
            reticleScale = tempScale;
        }

        public virtual void SetReticleHandler(System.Action overrideHandler)
        {
            reticleHandler = overrideHandler;
            reticle.SetActive(reticleHandler == null);            
        }

        public virtual void RestoreReticle()
        {
            reticle.SetActive(false);
            reticle = cacheReticle;
            reticle.SetActive(true);
            reticleScale = cacheReticleScale;
        }

        public virtual void Start()
        {
            if (!reticle)
                Debug.LogWarning("No reticle linked to PointerFeedback");
            if (!controllerRepresentation)
                Debug.LogWarning("No controller model linked to Pointerfeedback");
        }

        public virtual void SetReticlePosition(Vector3 pos)
        {
            pointerTarget = pos;
            if (reticle)
                reticle.transform.position = pointerTarget;
            reticleHandler?.Invoke();
        }

        public virtual void AlignReticleToCamera()
        {
            if (reticle)
            {
                reticle.transform.LookAt(mainCam.transform.position);
                reticle.transform.Rotate(0, 180f, 0);
            }
        }

        public virtual void SetReticleOrientation(Vector3 direction)
        {
            if (reticle)
            {
                reticle.transform.forward = direction;
                reticle.transform.Rotate(0, 180f, 0);
            }
        }

        public virtual void SetControllerOrientation(Vector3 pos, Quaternion rotation)
        {
            pointerPos = pos;
            pointerRot = rotation;
        }

        public virtual void SetHasController(bool has)
        {
            gotController = has;
        }

        public bool IsActive { get { return isActive; } }

        public virtual void OnFrame(VrEventInfo processedVrEventInfo)
        {
        }

        public virtual void DrawImmediate(Vector3 startPoint, Vector3 endPoint) { }

        public abstract void Activate(bool activate);
        public abstract void Draw(bool showBeam = true, bool showReticle = true);
    }
}
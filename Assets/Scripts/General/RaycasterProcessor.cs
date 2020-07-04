using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Obsolete
/// </summary>
namespace QS
{
    /// <summary>
    /// Obsolete. Using only PassiveRaycaster now
    /// </summary>
    public class RaycasterProcessor : InputProcessor
    {
        private GameObject uiTargetGameObject;
        private PointerEventData pointer;
        private List<RaycastResult> raycastResults;
        private VrEventInfo vrEventInfo;

        private readonly RaycastHit[] physicsRaycasts = new RaycastHit[1];

        public override void Awake()
        {
            base.Awake();

            pointer = new PointerEventData(EventSystem.current);

            Debug.Assert(pointer != null);

            raycastResults = new List<RaycastResult>();
        }

        public override VrEventInfo ProcessController(VrEventInfo info)
        {
            // Probably shouldn't cache this but pass in params but we need to use as reference

            vrEventInfo = info;
            UpdateRaycaster();

            switch (info.EventType)
            {
                case VrEventInfo.VrEventType.TriggerDown:
                    OnTriggerDown();
                    break;
                case VrEventInfo.VrEventType.TriggerUp:
                    OnTriggerUp();
                    break;
                case VrEventInfo.VrEventType.TouchpadClickDown:
                    OnPadClickDown(vrEventInfo.TouchpadPosition);
                    break;
                case VrEventInfo.VrEventType.TouchpadClickUp:
                    OnPadClickUp(vrEventInfo.TouchpadPosition);
                    break;
                case VrEventInfo.VrEventType.TouchpadTouchDown:
                    OnPadTouchDown(vrEventInfo.TouchpadPosition);
                    break;
                case VrEventInfo.VrEventType.TouchpadTouchUp:
                    OnPadTouchUp(vrEventInfo.TouchpadPosition);
                    break;
            }

            return vrEventInfo;
        }

        private void OnTriggerDown()
        {
            if (vrEventInfo.HitObject)
            {
                vrEventInfo.HitObject.OnTriggerClickDown(vrEventInfo);
                ControllerInput.Instance.SetGrabbedObject(vrEventInfo.HitObject);
            }
        }

        private void OnTriggerUp()
        {
            if (uiTargetGameObject)
            {
                // If a handler has been dropped on the submit button in the scene.
                // NOTE: this is not really desirable - more for testing - as finding
                // these handlers in the scene could get messy. Adding them in code
                // from a known place such as the current activity is fine
                ISubmitHandler submitHandler = uiTargetGameObject.GetComponent<ISubmitHandler>();
                if (submitHandler != null)
                    submitHandler.OnSubmit(pointer);

                vrEventInfo.EventType = VrEventInfo.VrEventType.UiSubmit;
            }
            else
            {
                if (vrEventInfo.GrabbedObject)
                {
                    vrEventInfo.GrabbedObject.OnTriggerClickUp(vrEventInfo);
                    ControllerInput.Instance.SetGrabbedObject(null);
                }
            }
        }

        private void OnPadClickDown(Vector2 position)
        {
            if (vrEventInfo.GrabbedObject)
                vrEventInfo.GrabbedObject.OnTouchpadClickDown(position);
        }

        private void OnPadClickUp(Vector2 position)
        {
            if (vrEventInfo.GrabbedObject)
                vrEventInfo.GrabbedObject.OnTouchpadClickUp(position);
        }

        private void OnPadTouchDown(Vector2 position)
        {
            if (vrEventInfo.GrabbedObject)
                vrEventInfo.GrabbedObject.OnTouchpadTouchDown(position);
        }

        private void OnPadTouchUp(Vector2 position)
        {
            if (vrEventInfo.GrabbedObject)
                vrEventInfo.GrabbedObject.OnTouchpadTouchUp(position);
        }

        /// <summary>
        /// Looks for any objects of interest and
        /// sets pointerTarget, which will either
        /// correspond with the raycast hit point
        /// or the end of the beam of specificed 
        /// length
        /// </summary>
        private void UpdateRaycaster()
        {
            // Set default as this is accessed for converting to 2D coords in CheckUiObjects
            vrEventInfo.RaycastHitPosition = vrEventInfo.ControllerPosition + vrEventInfo.ControllerDirection * ActivitySettings.Asset.raycastDistance;

            if (CheckUiObjects())
            {
                vrEventInfo.RaycastHit = true; // Do we need a separate UI cast hit flag? Probably not, as we're only interested in positioning the reticle
                vrEventInfo.RaycastNormal = -uiTargetGameObject.transform.forward;

                CheckGrabbableObject(null);
            }
            else if (Physics.RaycastNonAlloc(vrEventInfo.ControllerPosition, vrEventInfo.ControllerDirection, physicsRaycasts, 
                ActivitySettings.Asset.raycastDistance, ActivitySettings.Asset.interactableTarget) > 0)
            {
                vrEventInfo.RaycastHit = true;
                vrEventInfo.RaycastHitPosition = vrEventInfo.ControllerPosition.AbsoluteLerp(physicsRaycasts[0].point, ActivitySettings.Asset.inset);
                vrEventInfo.RaycastNormal = physicsRaycasts[0].normal;
                vrEventInfo.BeamDistance = physicsRaycasts[0].distance;

                CheckGrabbableObject(physicsRaycasts[0].transform.gameObject);
            }
            else
            {
                vrEventInfo.RaycastHit = false;                
                vrEventInfo.BeamDistance = ActivitySettings.Asset.raycastDistance;

                CheckGrabbableObject(null);
            }
        }

        /// <summary>
        /// Process status of Graspable
        /// </summary>
        /// <param name="hitObject"></param>
        private void CheckGrabbableObject(GameObject hitObject)
        {
            if (!hitObject)
            {
                if (vrEventInfo.HitObject)
                {
                    vrEventInfo.HitObject.OnPointerExit();
                    vrEventInfo.HitObject.Clear();
                    vrEventInfo.HitObject = null;
                    vrEventInfo.TargetObjectStatus = VrEventInfo.VrTargetObjectStatus.Unhighlighted;
                    ControllerInput.Instance.SetHighlightedObject(null);
                }
            }
            else
            {
                Graspable thisGrabbable = hitObject.GetComponent<Graspable>();
                if (thisGrabbable != vrEventInfo.HitObject)
                {
                    if (vrEventInfo.HitObject)
                    {
                        vrEventInfo.HitObject.OnPointerExit();
                        vrEventInfo.HitObject.Clear();
                    }
                    vrEventInfo.HitObject = thisGrabbable; // Might be null if no Graspable attached
                    if (vrEventInfo.HitObject)
                        vrEventInfo.HitObject.OnPointerEnter();
                    vrEventInfo.TargetObjectStatus = VrEventInfo.VrTargetObjectStatus.Highlighted;
                    ControllerInput.Instance.SetHighlightedObject(thisGrabbable);
                }
            }
        }

        /// <summary>
        /// Do a first-priority scan for Unity UI objects
        /// which will override any hits on general 
        /// Graspable gameobjects
        /// </summary>
        /// <returns></returns>
        private bool CheckUiObjects()
        {
            bool gotHit = false;

            if (pointer == null) // Not sure why this goes null
            {
                pointer = new PointerEventData(EventSystem.current);
                Debug.Assert(pointer != null);
            }

            pointer.position = mainCam.WorldToScreenPoint(vrEventInfo.RaycastHitPosition);

            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                Selectable selectable = null;
                RaycastResult raycastResult;
                for (int i = 0; i < raycastResults.Count; i++)
                {
                    raycastResult = raycastResults[i];
                    if (raycastResult.gameObject != uiTargetGameObject)
                    {
                        if (uiTargetGameObject)
                        {
                            Selectable existing = uiTargetGameObject.GetComponent<Selectable>();
                            if (existing)
                            {
                                existing.OnPointerExit(pointer);
                            }
                        }

                        uiTargetGameObject = raycastResult.gameObject;

                        selectable = uiTargetGameObject.GetComponent<Selectable>();

                        if (selectable)
                        {
                            selectable.OnPointerEnter(pointer);
                        }
                        else
                        {
                            uiTargetGameObject = null;
                        }
                    }

                    if (uiTargetGameObject)
                    {
                        gotHit = true;

                        // This is a hack because (after many years) Unity's RaycastResult.worldPos
                        // is still not calculated and is always zero
                        Vector3 worldPos = vrEventInfo.ControllerPosition + vrEventInfo.ControllerDirection * raycastResult.distance;
                        vrEventInfo.RaycastHitPosition = vrEventInfo.ControllerPosition.AbsoluteLerp(worldPos, ActivitySettings.Asset.inset);
                        vrEventInfo.BeamDistance = raycastResult.distance;
                        break; // Only interested in frontmost raycast for ui
                    }
                }
            }
            else if (uiTargetGameObject)
            {
                Selectable existing = uiTargetGameObject.GetComponent<Selectable>();

                if (existing)
                {
                    existing.OnPointerExit(pointer);
                }

                uiTargetGameObject = null;
            }

            return gotHit;
        }
    }
}

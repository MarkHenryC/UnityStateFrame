using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QS
{
    public class PassiveRaycast : InputProcessor
    {
        private PointerEventData pointer;
        private List<RaycastResult> raycastResults;

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
            return UpdateRaycaster(info);
        }

        /// <summary>
        /// Looks for any objects of interest and
        /// sets RaycastHitPosition, which will either
        /// correspond with the raycast hit point
        /// or the end of the beam of specificed 
        /// length
        /// </summary>
        private VrEventInfo UpdateRaycaster(VrEventInfo vrEventInfo)
        {
            vrEventInfo.PrevHitObject = vrEventInfo.HitObject;
            vrEventInfo.genericHitObject = null;
#if USING_UUI
            if (CheckUiObjects(ref vrEventInfo))
            {
                vrEventInfo.RaycastHit = true; // Do we need a separate UI cast hit flag? Probably not, as we're only interested in positioning the reticle
                vrEventInfo.RaycastNormal = -vrEventInfo.UiTargetGameObject.transform.forward;
                vrEventInfo.HitObject = null;
            }
            else 
#endif
            bool gotHit = false;
            if (Physics.RaycastNonAlloc(vrEventInfo.ControllerPosition, vrEventInfo.ControllerDirection, physicsRaycasts,
                ActivitySettings.Asset.raycastDistance, ActivitySettings.Asset.interactableTarget) > 0)
            {
                gotHit = true;

                Graspable hitObject = physicsRaycasts[0].transform.gameObject.GetComponent<Graspable>();
                if (!hitObject) // This might be a child 'trap' for a raycast such as a label
                    hitObject = physicsRaycasts[0].transform.gameObject.GetComponentInParent<Graspable>();

                // Ignore anything with a Graspable that's marked as dormant
                if (hitObject && hitObject.Dormant)
                    gotHit = false;

                if (gotHit)
                {
                    vrEventInfo.RaycastHit = true;
                    vrEventInfo.RaycastHitPosition = physicsRaycasts[0].point;
                    vrEventInfo.RaycastNormal = physicsRaycasts[0].normal;
                    vrEventInfo.BeamDistance = physicsRaycasts[0].distance;
                    vrEventInfo.PointerDotWithUp = Vector3.Dot(vrEventInfo.ControllerDirection, Vector3.up);
                    vrEventInfo.HitObject = hitObject;
                    vrEventInfo.genericHitObject = physicsRaycasts[0].transform.gameObject;
                }
            }

            if (!gotHit)
            {
                vrEventInfo.RaycastHit = false;
                vrEventInfo.RaycastHitPosition = vrEventInfo.ControllerPosition + vrEventInfo.ControllerDirection * ActivitySettings.Asset.raycastDistance;
                vrEventInfo.BeamDistance = ActivitySettings.Asset.raycastDistance;
                vrEventInfo.HitObject = null;
            }

            return vrEventInfo;
        }


        /// <summary>
        /// Do a first-priority scan for Unity UI objects
        /// which will override any hits on general 
        /// Graspable gameobjects. Not a totally passive
        /// scan, as UI hits are dispatched immediately.
        /// It wouldn't fit well with the Unity UI model
        /// to defer dispatching events.
        /// </summary>
        /// <returns></returns>
        private bool CheckUiObjects(ref VrEventInfo vrEventInfo)
        {
            if (pointer == null) // Not sure why this goes null sometimes
            {
                pointer = new PointerEventData(EventSystem.current);
                Debug.Assert(pointer != null);
            }

            pointer.position = mainCam.WorldToScreenPoint(vrEventInfo.RaycastHitPosition);

            EventSystem.current.RaycastAll(pointer, raycastResults);
            vrEventInfo.newSelected = null;

            if (raycastResults.Count > 0)
            {
                RaycastResult raycastResult;
                int raycastCount = raycastResults.Count;

#pragma warning disable CS0162 // Unreachable code detected
                for (int i = 0; i < raycastCount; i++)
#pragma warning restore CS0162 // Unreachable code detected
                {
                    raycastResult = raycastResults[i];
                    if (raycastResult.gameObject != vrEventInfo.UiTargetGameObject)
                    {
                        if (vrEventInfo.UiTargetGameObject)
                        {
                            vrEventInfo.previousSelected = vrEventInfo.UiTargetGameObject.GetComponent<Selectable>();
                            if (vrEventInfo.previousSelected)
                                vrEventInfo.previousSelected.OnPointerExit(pointer);
                        }

                        vrEventInfo.UiTargetGameObject = raycastResult.gameObject;
                        vrEventInfo.newSelected = raycastResult.gameObject.GetComponent<Selectable>();
                        vrEventInfo.EventType = VrEventInfo.VrEventType.UiPointerEnter;

                        // This is a hack because (after many years) Unity's RaycastResult.worldPos
                        // is still not calculated and is always zero.
                        Vector3 worldPos = vrEventInfo.ControllerPosition + vrEventInfo.ControllerDirection * raycastResult.distance;
                        vrEventInfo.RaycastHitPosition = vrEventInfo.ControllerPosition.AbsoluteLerp(worldPos, ActivitySettings.Asset.inset);
                        vrEventInfo.BeamDistance = raycastResult.distance;
                    }
                    return true; // Only interested in frontmost raycast for ui

                }
            }
            else if (vrEventInfo.UiTargetGameObject)
            {
                vrEventInfo.previousSelected = vrEventInfo.UiTargetGameObject.GetComponent<Selectable>();
                vrEventInfo.EventType = VrEventInfo.VrEventType.UiPointerExit;
                vrEventInfo.UiTargetGameObject = null;

                if (vrEventInfo.previousSelected)
                    vrEventInfo.previousSelected.OnPointerExit(pointer);
            }

            return false;
        }
    }
}

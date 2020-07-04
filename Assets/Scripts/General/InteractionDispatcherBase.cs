using UnityEngine;

namespace QS
{
    public class InteractionDispatcherBase : MonoBehaviour
    {
        public virtual VrEventInfo DispatchInteraction(VrEventInfo vrEventInfo)
        {
            vrEventInfo = CheckGrabbableObject(vrEventInfo);
            switch (vrEventInfo.EventType)
            {
                case VrEventInfo.VrEventType.TriggerDown:
                    if (vrEventInfo.HitObject)
                    {
                        vrEventInfo.HitObject.OnTriggerClickDown(vrEventInfo);
                        if (vrEventInfo.PrevHitObject && vrEventInfo.PrevHitObject != vrEventInfo.HitObject)
                        {
                            vrEventInfo.PrevHitObject.OnPointerExit();
                            vrEventInfo.PrevHitObject.Clear();
                            vrEventInfo.PrevHitObject = null;
                        }

                        // This looks a bit hackish, but where else to trap
                        // Teleport, as I'm doing it on trigger-up which is
                        // a bit less disorienting to the player.

                        if (!vrEventInfo.HitObject.IsA<Teleportable>() && !vrEventInfo.HitObject.IsButton())
                            vrEventInfo.GrabbedObject = vrEventInfo.HitObject;
                    }
                    break;
                case VrEventInfo.VrEventType.TriggerUp:
                    if (vrEventInfo.GrabbedObject)
                        vrEventInfo.GrabbedObject.OnTriggerClickUp(vrEventInfo);
                    else if (vrEventInfo.HitObject && vrEventInfo.HitObject.IsButton())
                        vrEventInfo.HitObject.OnTriggerClickUp(vrEventInfo);
                    else if (vrEventInfo.HitObject.IsA<Teleportable>() && vrEventInfo.PointerDotWithUp >= ActivitySettings.Asset.minimumAngleForTeleport)
                            vrEventInfo.EventType = VrEventInfo.VrEventType.Teleport;
                    else if (vrEventInfo.newSelected)
                        vrEventInfo.newSelected.OnPointerUp(vrEventInfo.pointerEventData);
                    break;
                case VrEventInfo.VrEventType.TouchpadClickDown:
                    if (vrEventInfo.GrabbedObject)
                        vrEventInfo.GrabbedObject.OnTouchpadClickDown(vrEventInfo.TouchpadPosition);
                    break;
                case VrEventInfo.VrEventType.TouchpadClickUp:
                    if (vrEventInfo.GrabbedObject)
                        vrEventInfo.GrabbedObject.OnTouchpadClickUp(vrEventInfo.TouchpadPosition);
                    break;
                case VrEventInfo.VrEventType.TouchpadTouchDown:
                    if (vrEventInfo.GrabbedObject)
                        vrEventInfo.GrabbedObject.OnTouchpadTouchDown(vrEventInfo.TouchpadPosition);
                    break;
                case VrEventInfo.VrEventType.TouchpadTouchUp:
                    if (vrEventInfo.GrabbedObject)
                        vrEventInfo.GrabbedObject.OnTouchpadTouchUp(vrEventInfo.TouchpadPosition);
                    break;
            }

            return vrEventInfo;
        }

        /// <summary>
        /// Process status of Graspable
        /// </summary>
        /// <param name="hitObject"></param>
        protected virtual VrEventInfo CheckGrabbableObject(VrEventInfo vrEventInfo)
        {
            if (!vrEventInfo.HitObject)
            {
                if (vrEventInfo.PrevHitObject)
                {
                    vrEventInfo.PrevHitObject.OnPointerExit();
                    vrEventInfo.PrevHitObject.Clear();
                    vrEventInfo.PrevHitObject = null;
                    vrEventInfo.TargetObjectStatus = VrEventInfo.VrTargetObjectStatus.Unhighlighted;
                }
            }
            else
            {
                if (vrEventInfo.HitObject != vrEventInfo.PrevHitObject)
                {
                    if (vrEventInfo.PrevHitObject)
                    {
                        vrEventInfo.PrevHitObject.OnPointerExit();
                        vrEventInfo.PrevHitObject.Clear();
                        vrEventInfo.PrevHitObject = null;
                    }

                    bool okHit = false;
                    if (vrEventInfo.HitObject)
                    {
                        if (vrEventInfo.HitObject.IsA<Teleportable>())
                        {
                            if (vrEventInfo.PointerDotWithUp >= ActivitySettings.Asset.minimumAngleForTeleport)
                                okHit = true;
                        }
                        else
                            okHit = true;
                    }
                    if (okHit)
                    {
                        vrEventInfo.HitObject.OnPointerEnter();
                        vrEventInfo.TargetObjectStatus = VrEventInfo.VrTargetObjectStatus.Highlighted;
                    }
                }
            }

            return vrEventInfo;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class InteractionDispatcherHolding : InteractionDispatcherBase
    {
        /// <summary>
        /// Differs from base in that we're not checking rollovers as
        /// we've attached the grabbed object. So only interested
        /// in releasing the object
        /// </summary>
        /// <param name="vrEventInfo"></param>
        /// <returns></returns>
        public override VrEventInfo DispatchInteraction(VrEventInfo vrEventInfo)
        {
            switch (vrEventInfo.EventType)
            {
                case VrEventInfo.VrEventType.TriggerUp:
                    if (vrEventInfo.GrabbedObject)
                        vrEventInfo.GrabbedObject.OnTriggerClickUp(vrEventInfo);
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

    }
}
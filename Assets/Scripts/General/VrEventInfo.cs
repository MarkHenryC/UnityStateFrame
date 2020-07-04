using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QS
{
    public class VrEventInfo
    {
        private const float TouchpadZoneRatio = 0.75f;

        public enum VrEventType
        {
            None,
            TriggerDown,
            TriggerUp,
            TouchpadClickDown,
            TouchpadClickUp,
            TouchpadTouchDown,
            TouchpadTouchUp,
            BackButton,
            UiPointerEnter,
            UiPointerExit,
            UiSubmit,
            Teleport // created by catching a trigger up on a teleportable object
        };

        public enum VrTargetObjectStatus
        {
            None,
            Highlighted,
            Unhighlighted,
        };

        /// <summary>
        /// We simulate arrow keys in Go controller
        /// by quantising the pad position of the click
        /// </summary>
        public enum VrArrowKey
        {
            None,
            Left,
            Right,
            Up,
            Down
        };

        public VrEventType EventType { set; get; } // Our internal event identification
        public VrTargetObjectStatus TargetObjectStatus { set; get; } // UI state
        public VrArrowKey ArrowKey { set; get; }
        public ISubmitHandler UiSubmitHandler {set;get;} // The handler attached to a UI button
        public Selectable previousSelected, newSelected; // Cache previous UI selection so we can trigger a rolloff
        public Quaternion ControllerRotation { set; get; } // Aspect of the controller
        public bool Connected { set; get; } // Is a real controller connected
        public bool RaycastHit { set; get; } // Did we just get a hit
        public bool TriggerIsDown { set; get; } // Is the Go controller trigger key down. Left mouse in editor
        public bool TouchIsDown { set; get; } // Is Go touchpad button clicked down
        public bool TouchIsTouched { set; get; } // Is thumb on touchpad. In editor this is simulated with right mouse
        public bool NewGrabbable { set; get; } // Hint that a new object (or a change to nothing) has been grabbed
        public Vector3 ControllerPosition { set; get; } // Go controller position (preset according to handledness)
        public Vector3 ControllerDirection { set; get; } // Go controller pointing direction
        public Vector3 RaycastHitPosition { set; get; } // Normally where we'd put the pointer cue such as reticle
        public Vector3 RaycastNormal { set; get; } // The normal of the polygon that was hit in the raycast
        public Vector3 PlayerPosition { set; get; } // The camera
        public Vector2 TouchpadPosition { set; get; } // Current position
        public Vector2 PrevTouchpadPosition { set; get; } // Position previous frame reading        
        public Vector3 HoldingTargetPosition { set; get; } // The position of the currently-grabbed object
        public Vector3 AngularAccelleration { set; get; } // Mostly for sfx when moving something
        public Vector3 ClickdownOffset { set; get; } // Click position minus object's position; add this back when moving
        public Graspable HitObject { set; get; } // Was hit in a raycast; may be the same as GrabbedObject
        public Graspable PrevHitObject { set; get; } // Cache previous hit so when there's a new one we can call the exit method
        public GameObject UiTargetGameObject, genericHitObject;
        public PointerEventData pointerEventData; // Cache this from the UI routine
        public float BeamDistance { set; get; } // Usually to the hit object, or projected along the controller's forward vector at the preset raycast distance
        public float PointerDotWithUp { set; get; } // While doing a raycast, store the dot product of the pointer and the up vector

        private Graspable grabbedObject;
        private Rigidbody grabbedRb;
        private bool grabbedRbvHasGravity;

        public void ClearTouchpad()
        {
            TouchpadPosition = Vector2.zero;
        }

        public VrEventInfo()
        {
            EventType = VrEventType.None;
            ClearTouchpad();
        }

        /// <summary>
        /// Where the pointer is in space; mostly useful
        /// when an object is clicked
        /// </summary>
        public Vector3 PointerTarget
        {
            get { return ControllerPosition + ControllerDirection * BeamDistance; }
        }

        /// <summary>
        /// Calc done at controller end for 
        /// consumption in current state. Note
        /// that we're only interested in touch
        /// moves, not touchpad clickdown moves.
        /// </summary>
        public Vector2 TouchpadMoved
        {
            get
            {
                if (TouchpadPosition != Vector2.zero && 
                    PrevTouchpadPosition != Vector2.zero && TouchIsTouched)
                    return TouchpadPosition - PrevTouchpadPosition;
                else
                    return Vector2.zero;
            }
        } 

        public Graspable GrabbedObject
        {
            set
            {
                if (value != GrabbedObject)
                {
                    NewGrabbable = true;
                    if (grabbedObject)
                        grabbedObject.Clear();
                    grabbedObject = value;
                    if (grabbedObject)
                    {
                        grabbedRb = GrabbedObject.GetComponent<Rigidbody>();
                        if (grabbedRb)
                            grabbedRbvHasGravity = grabbedRb.useGravity;
                    }
                    else
                    {
                        if (grabbedRb)
                            grabbedRb.useGravity = grabbedRbvHasGravity;
                        grabbedRb = null;
                    }
                }
            }

            get
            {
                return grabbedObject;
            }
        }

        /// <summary>
        /// Cache a rigidbody on the grabbed object
        /// if there is one attached
        /// </summary>
        public Rigidbody GrabbedRb
        {
            get { return grabbedRb; }
        }

        /// <summary>
        /// Clear things that need to be evaluated
        /// each frame
        /// </summary>
        public void ClearTemporal()
        {
            EventType = VrEventType.None;
            TargetObjectStatus = VrTargetObjectStatus.None;
            RaycastHit = false;
            NewGrabbable = false;
        }

        /// <summary>
        /// Return a 4-way-switch-style reading
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public VrEventInfo.VrArrowKey GetArrowKey()
        {
            float absX = Mathf.Abs(TouchpadPosition.x);
            float absY = Mathf.Abs(TouchpadPosition.y);

            if (absX > absY)
            {
                if (absX > TouchpadZoneRatio)
                    return TouchpadPosition.x < 0f ? VrEventInfo.VrArrowKey.Left : VrEventInfo.VrArrowKey.Right;
            }
            else
            {
                // Need to do a little fiddle here as the 
                // thumb covers a bigger area of the 
                // touchpad. Not an issue with low Y, 
                // but an issue when thumbing the upper
                // area as the Go controller seems to
                // prioritise the lowest contact point.
                if (TouchpadPosition.y < -TouchpadZoneRatio)
                    return VrArrowKey.Down;
                else if (TouchpadPosition.y > 0f)
                    return VrEventInfo.VrArrowKey.Up;
            }
            return VrEventInfo.VrArrowKey.None;
        }
    }
}
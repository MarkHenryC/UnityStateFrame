using UnityEngine;
using TMPro;
using System;

namespace QS
{
    /// <summary>
    /// Inherit from this to determine local
    /// behaviour of Graspable object. The
    /// object doesn't do anything outside
    /// visual feedback. Any hits on 
    /// a Graspable are recorded in the 
    /// VrEventInfo class by the active
    /// InputProcessor class such as
    /// PassiveRaycaster
    /// </summary>
    public class Graspable : MonoBehaviour
    {
        [Tooltip("A static GameObject that shows and hides; no settable text")]
        public GameObject rolloverPrompt, clickdownPrompt;
        public float clickdownInfoTimeout = 4f; // If we're manipulating object we want the text to go away
        public Vector3 homePosition, destPosition;
        public Quaternion homeRotation, destRotation;
        public Action<Graspable> rolloverAction, rolloffAction, clickDownAction, clickUpAction;
        public bool returnToHomeIfFallen; // Generally means it's pushed through a wall if rigidbody
        public bool returnToHomeIfInvisible; // Generally means it's pushed through a wall if non-rb        
        public bool IsDormant; // Hittable but not moveable
        public bool promptsFixed;

        public float minumYPosition = 0f;
        [Tooltip("A shared infoPanel that displays rolloverTextData and clickdownTextData")]
        public InfoPanel sharedInfo;
        [Tooltip("Text to display by sharedInfo")]
        public string rolloverTextData, clickdownTextData;
        public ITextReceiver textReceiver;

        //public Bounds
        protected bool pointerWithin;
        protected bool triggerDown;
        protected bool touchpadDown;
        protected bool touchpadTouching;
        protected bool isSelectedStart, isSelectedEnd; // for testing a toggle-select       
        protected bool ignoreRollovers;
        protected FixedJoint attachedJoint; // for attaching and moving
        protected Rigidbody attachedRb; // Optional
        protected Rigidbody cache_attachedRb;
        protected float clickdownTimer;
        protected Tooltip tooltip;
        protected bool isKinematic;
        protected TextMeshPro clickdownText;
        protected Vector3 clickdownOffset = Vector3.zero;
        
        protected virtual void Awake()
        {
            attachedRb = GetComponent<Rigidbody>();
            tooltip = GetComponentInChildren<Tooltip>();
            if (attachedRb)
                isKinematic = attachedRb.isKinematic;
            if (clickdownPrompt)
                clickdownText = clickdownPrompt.GetComponent<TextMeshPro>();
            if (rolloverPrompt)
                rolloverPrompt.SetActive(false);
            if (clickdownPrompt)
                clickdownPrompt.SetActive(false);
            minumYPosition = transform.position.y;
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            if (clickdownPrompt && clickdownPrompt.activeSelf)
            {
                if (!promptsFixed)
                    Utils.FaceCamera(clickdownPrompt.transform, ControllerInput.Instance.Player);
                clickdownTimer += Time.deltaTime;
                if (clickdownTimer > clickdownInfoTimeout)
                    clickdownPrompt.SetActive(false);
            }

            if (rolloverPrompt && rolloverPrompt.activeSelf && !promptsFixed)
                Utils.FaceCamera(rolloverPrompt.transform, ControllerInput.Instance.Player);

            if (returnToHomeIfFallen)
            {
                if (transform.position.y < minumYPosition)
                    TransformToHome();
            }
        }

        public virtual void SetHomeTransform(bool local = false)
        {
            if (local)
            {
                homePosition = transform.localPosition;
                homeRotation = transform.localRotation;
            }
            else
            {
                homePosition = transform.position;
                homeRotation = transform.rotation;
            }            
        }

        public virtual void TransformToHome(bool local = false)
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

        public virtual void SetDestTransform(bool local = false)
        {
            if (local)
            {
                destPosition = transform.localPosition;
                destRotation = transform.localRotation;
            }
            else
            {
                destPosition = transform.position;
                destRotation = transform.rotation;
            }
        }

        public virtual void TransformToDest(bool local = false)
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

        public virtual void TransformTo(Vector3 destPos, Quaternion destRot, bool local = false)
        {
            if (local)
            {
                transform.localPosition = destPos;
                transform.localRotation = destRot;
            }
            else
            {
                transform.position = destPos;
                transform.rotation = destRot;
            }
        }

        /// <summary>
        /// Such as when object is dropped and
        /// about to be disabled. Don't allow
        /// player to click on it when it's in
        /// a transition state. Late mod:
        /// attach to a public serializable
        /// variable to make it easy to 
        /// use grabbales as masks
        /// </summary>
        public bool Dormant
        {
            set { IsDormant = value; }
            get { return IsDormant; }
        }

        /// <summary>
        /// Should be at the top level as
        /// there's no need to override
        /// rigidbody functionality. And
        /// the RB is at the same level as
        /// the Graspable component rather
        /// than at the actual model level
        /// </summary>
        public bool Gravity
        {
            set
            {
                var c = GetComponent<Rigidbody>();
                if (c)
                    c.useGravity = value;
            }
            get
            {
                var c = GetComponent<Rigidbody>();
                if (c)
                    return c.useGravity;
                else
                    return false;
            }
        }

        public bool Kinematic
        {
            set
            {
                var c = GetComponent<Rigidbody>();
                if (c)
                    c.isKinematic = value;
            }
            get
            {
                var c = GetComponent<Rigidbody>();
                if (c)
                    return c.isKinematic;
                else
                    return false;
            }
        }

        /// <summary>
        /// Will be null if not connected to
        /// a joint. By using the actual joint
        /// rather than a bool means we can
        /// detach it externally
        /// </summary>
        public FixedJoint AttachedJoint
        {
            get { return attachedJoint; }
        }

        public virtual bool IsSelected()
        {
            return triggerDown && pointerWithin;
        }

        protected void SetTooltip(string text = "")
        {
            if (tooltip)
            {
                tooltip.SetText(text);
                if (!promptsFixed)
                    Utils.FaceCamera(tooltip.transform);
            }
        }

        protected void SetClickDownText(string text)
        {
            if (clickdownText)
                clickdownText.text = text;
        }

        /// <summary>
        /// A bit hacky, but I don't want to 
        /// be scanning for multiple component
        /// types in raycasts, so we can use
        /// the Graspable functionality in 
        /// buttons. Just override, as is
        /// done in the ButtonPanel class
        /// </summary>
        /// <returns></returns>
        public virtual bool IsButton()
        {
            return false;
        }

        public virtual void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public virtual bool IsVisible()
        {
            return gameObject.activeInHierarchy;
        }

        public virtual void EnableRb(bool enable)
        {
            if (!enable)
            {
                if (attachedRb)
                {
                    attachedRb.isKinematic = true;
                    attachedRb.detectCollisions = false;
                }
                cache_attachedRb = attachedRb;
                attachedRb = null;
            }
            else
            {
                attachedRb = cache_attachedRb;
                if (attachedRb)
                {
                    attachedRb.isKinematic = isKinematic;
                    attachedRb.detectCollisions = true;
                }
            }
        }

        public virtual void OnPointerEnter()
        {
            pointerWithin = true;
            if (rolloverPrompt)
            {
                rolloverPrompt.SetActive(true);
                if (!promptsFixed)
                    Utils.FaceCamera(rolloverPrompt.transform, ControllerInput.Instance.Player);
            }

            if (sharedInfo && rolloverTextData.Usable())
                sharedInfo.SetText(rolloverTextData);

            textReceiver?.ReceiveRolloverText(rolloverTextData);

            rolloverAction?.Invoke(this);
        }

        public virtual void OnPointerExit()
        {
            pointerWithin = false;

            if (rolloverPrompt)
                rolloverPrompt.SetActive(false);

            if (sharedInfo)
                sharedInfo.Show(false);

            textReceiver?.ReceiveRolloverText(null);

            rolloffAction?.Invoke(this);
        }

        public virtual void OnTriggerClickDown(VrEventInfo info)
        {
            triggerDown = true;

            clickdownOffset = info.PointerTarget - transform.position;
            
            if (rolloverPrompt)
                rolloverPrompt.SetActive(false);

            if (sharedInfo && clickdownTextData.Usable())
                sharedInfo.SetText(clickdownTextData);

            textReceiver?.ReceiveClickdownText(clickdownTextData);

            if (clickdownPrompt)
            {
                clickdownPrompt.SetActive(true);
                if (!promptsFixed)
                    Utils.FaceCamera(clickdownPrompt.transform, ControllerInput.Instance.Player);

                clickdownTimer = 0f;
            }

            ClickDownEffect();

            clickDownAction?.Invoke(this);
        }

        public virtual void OnTriggerClickUp(VrEventInfo info)
        {
            triggerDown = false;
            if (clickdownPrompt)
                clickdownPrompt.SetActive(false);

            if (sharedInfo)
                sharedInfo.Show(false);

            textReceiver?.ReceiveClickdownText(null);

            if (attachedJoint)
            {
                attachedJoint.connectedBody = null;
                attachedJoint = null;
            }

            ClickUpEffect();

            clickUpAction?.Invoke(this);
        }

        public virtual void ClickDownEffect()
        {
            if (!Dormant)
                ControllerInput.Instance.ClickGrabSound();
        }

        public virtual void ClickUpEffect()
        {
            if (!Dormant)
                ControllerInput.Instance.ClickReleaseSound();
        }

        public virtual void OnTouchpadClickDown(Vector2 pos)
        {
            touchpadDown = true;
        }

        public virtual void OnTouchpadClickUp(Vector2 pos)
        {
            touchpadDown = false;
        }

        public virtual void OnTouchpadTouchDown(Vector2 pos)
        {
            touchpadTouching = true;
        }

        public virtual void OnTouchpadTouchUp(Vector2 pos)
        {
            touchpadTouching = false;
        }

        public virtual void MoveTo(Vector3 newPos)
        {
            if (attachedRb)
                attachedRb.MovePosition(newPos - clickdownOffset);
            else
                transform.position = newPos - clickdownOffset;
        }

        /// <summary>
        /// This is a specific action based on a
        /// touchpad swipe
        /// </summary>
        /// <param name="amount"></param>
        public virtual void HandleHorizontalSwipe(float amount)
        {
            // Reverse Y rotation as it looks better when it follows
            // direction of thumb swipe from player's perspective
            Rotate(0, -amount, 0);
        }

        /// <summary>
        /// Some subclasses such as Twistable may want to
        /// do something with the raw controller rotation
        /// </summary>
        /// <param name="rotation"></param>
        public virtual void UpdateRotation(Quaternion rotation)
        {

        }

        /// <summary>
        /// This is a rotation offset with a rigid body
        /// is attached, otherwise a simple rotation
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="ey"></param>
        /// <param name="ez"></param>
        public virtual void Rotate(float ex, float ey, float ez)
        {
            if (attachedRb)
            {
                Vector3 curRot = transform.rotation.eulerAngles;
                attachedRb.MoveRotation(Quaternion.Euler(curRot.x - ex, curRot.y - ey, curRot.z - ez));
            }
            else
                transform.Rotate(ex, ey, ez);
        }

        public virtual void Rotate(Vector3 euler)
        {
            Rotate(euler.x, euler.y, euler.z);
        }

        public virtual void Clear()
        {
            pointerWithin = false;
            triggerDown = false;
            touchpadDown = false;
            touchpadTouching = false;
        }

        public virtual FixedJoint AttachToJoint(GameObject jointObject)
        {
            // Remove any existing connection. This also means
            // that we can clear a connection by passing a
            // null parameter

            if (attachedJoint)
            {
                attachedJoint.connectedBody = null;
                attachedJoint = null;
            }

            if (jointObject)
                attachedJoint = Utils.JointToRigidbody(jointObject, gameObject);

            return attachedJoint;
        }
    }
}
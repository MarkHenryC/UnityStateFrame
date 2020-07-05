using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QS
{
    /// <summary>
    /// Async handler for all interactions.
    /// </summary>
    public class ControllerInput : Singleton<ControllerInput>
    {
        [Tooltip("Vertical drop for camera pos")]
        public float cameraOffset = 0.1f;
        public PointerHandler beamPointerHandler, grabPointerHandler, nullPointerHandler;
        public Transform player;
        public Vector3 controllerOffsetDesktop; // For testing
        public Hud hud;
        public QuiteSensible.TrackPlayer bgMusic;
        public QuiteSensible.TrackPlayer voiceover;
        public GameObject touchpadPrompt;
        public GameObject touchpadNorth, touchpadSouth, touchpadCentre;
        public float touchpadFlashInterval = 0.5f;
        public AudioClip clickGrab, clickRelease;
        public GameObject moveGuide;
        public UiPanel backButtonPanel;
        public TextMeshProUGUI readout;
        public ScreenFader screenFade;

        private const float MousewheelRotationCoeff = 20f;
        private const int periodicUiUpdate = 20;

        [System.NonSerialized]
        public PointerHandler currentPointerHandler;

        public enum EnPointerMode { None, Pointing, Holding, Ui, LockedObject, CustomBeam };

        private EnPointerMode pointerMode = EnPointerMode.None;
        private Camera mainCam;
        private VrEventInfo vrEventInfo = new VrEventInfo();
        private Graspable currentGrabbedObject, currentHighlightedObject;
        private List<Graspable> selectedGrabbables = new List<Graspable>();
        private Vector3 lastControllerPosition, lastControllerDirection; // For quiclk queries
        private Vector3 calcOffset = Vector3.zero;
        private Rigidbody playerPhysics;
        private CapsuleCollider playerCollider; // need to switch off when in car parking mode
        private bool flagTriggerRelease;
        private bool freeMove;
        private Quaternion globalPointerRot;
        private bool paused;
        private float unscaledTimer;
        private float simRotationAngle;
        private Transform cameraPos;
        // Caching for speed as it's used every frame
        private int lowPassSamples;
        private int periodicFrameCounter;
        private bool touchdown; // Monitor via clicks

        private const float PAUSE_TIME = 5f;

        // Clamped controller z axis twist
        public float UnitRotationZ { get; private set; }
        // Runtime checks by stuff like car sim
        public bool TriggerIsDown => IsTriggerDown();
        public bool TouchIsDown => IsTouchpadDown();
        public bool TouchIsTouched => IsTouchpadTouched();

        private class QData
        {
            public Vector3 forward, up;
            public QData(Vector3 f, Vector3 u)
            {
                forward = f;
                up = u;
            }

            public QData() : this(new Vector3(), new Vector3())
            {

            }

            public Quaternion ToQ()
            {
                return Quaternion.LookRotation(forward, up);
            }
        }

        private void Start()
        {
            mainCam = Camera.main;
            playerPhysics = player.GetComponent<Rigidbody>();
            if (playerPhysics)
                LockPlayer(true);
            playerCollider = player.GetComponent<CapsuleCollider>();
            cameraPos = player.Find("CenterEyeAnchor");
            PointerMode = EnPointerMode.Pointing;
            lowPassSamples = ActivitySettings.Asset.lowPassSamples;

            Debug.LogFormat("Start called in ControllerInput in scene {0}", SceneManager.GetActiveScene().name);

            Debug.Log("Device present? " + UnityEngine.XR.XRDevice.isPresent);
            Debug.Log("VR enabled? " + UnityEngine.XR.XRSettings.enabled);

            ActivitySettings.Asset.ResetCurrentExperienceScores();

            if (!ActivitySettings.Asset.showTouchpadY)
                readout.gameObject.SetActive(false);

            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
            OVRManager.display.displayFrequency = 72.0f;

        }

        private void Update()
        {
            bool connected = false;
            Vector3 pointerDir = Vector3.zero, angularAcceleration = Vector3.zero;
            Vector3 globalPointerPos = Vector3.zero;


            vrEventInfo.ClearTemporal();

#if UNITY_ANDROID && !UNITY_EDITOR
            Vector3 localPointerPos;
            Quaternion localPointerRot;

            OVRInput.Controller activeController = OVRInput.GetActiveController();
            if (activeController == OVRInput.Controller.LTrackedRemote || activeController == OVRInput.Controller.RTrackedRemote)
            {
                localPointerPos = OVRInput.GetLocalControllerPosition(activeController);
                localPointerRot = OVRInput.GetLocalControllerRotation(activeController);
                globalPointerPos = mainCam.transform.parent.TransformPoint(localPointerPos);
                angularAcceleration = OVRInput.GetLocalControllerAngularAcceleration(activeController);
                globalPointerRot = player.rotation * localPointerRot;
                connected = true;
            }
            
            globalPointerRot = Utils.SmoothQuaternion(globalPointerRot, lowPassSamples);

            // New: calc after rotation smoothing
            pointerDir = globalPointerRot * Vector3.forward;

#else
            connected = false;

            calcOffset = mainCam.transform.right * controllerOffsetDesktop.x + mainCam.transform.up *
                controllerOffsetDesktop.y + mainCam.transform.forward * controllerOffsetDesktop.z;

            float deltaMouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            simRotationAngle += deltaMouseScrollWheel * MousewheelRotationCoeff;
            Vector3 camAngles = Camera.main.transform.rotation.eulerAngles;
            camAngles.z = simRotationAngle;
            globalPointerRot = Quaternion.Euler(camAngles);

            globalPointerPos = mainCam.transform.position + calcOffset;
            pointerDir = mainCam.transform.parent.TransformDirection(mainCam.transform.localRotation * Vector3.forward);
            angularAcceleration = Vector3.zero;

            if (vrEventInfo.Connected)
            {
                vrEventInfo.Connected = false;
                vrEventInfo.Connected = false;
            }

#endif
            if (connected && !vrEventInfo.Connected)
                vrEventInfo.Connected = true;

            lastControllerPosition = globalPointerPos;
            lastControllerDirection = pointerDir;

            vrEventInfo.ControllerPosition = globalPointerPos;
            vrEventInfo.ControllerRotation = globalPointerRot;
            vrEventInfo.ControllerDirection = pointerDir;
            vrEventInfo.AngularAccelleration = angularAcceleration;

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TriggerDown;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                // Real controller is likely to be returned to default rotation
                // so reset in desktop mode
                simRotationAngle = 0f;
                vrEventInfo.ControllerRotation = Quaternion.identity;
                vrEventInfo.EventType = VrEventInfo.VrEventType.TriggerUp;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadTouchDown;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadTouchUp;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickDown;
                vrEventInfo.TouchpadPosition = Vector2.up;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickDown;
                vrEventInfo.TouchpadPosition = Vector2.down;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickDown;
                vrEventInfo.TouchpadPosition = Vector2.left;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickDown;
                vrEventInfo.TouchpadPosition = Vector2.right;
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickUp;
                vrEventInfo.TouchpadPosition = Vector2.up;
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickUp;
                vrEventInfo.TouchpadPosition = Vector2.down;
            }
            else if (Input.GetKeyUp(KeyCode.A))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickUp;
                vrEventInfo.TouchpadPosition = Vector2.left;
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickUp;
                vrEventInfo.TouchpadPosition = Vector2.right;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.BackButton;
                OnBackButtonClickUp();
            }

#else
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TriggerDown;
            }
            else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TriggerUp;
            }
            else if (OVRInput.GetDown(OVRInput.Button.One))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickDown;
            }
            else if (OVRInput.GetUp(OVRInput.Button.One))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadClickUp;
            }
            else if (OVRInput.GetDown(OVRInput.Touch.One))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadTouchDown;
            }
            else if (OVRInput.GetUp(OVRInput.Touch.One))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.TouchpadTouchUp;
            }
            else if (OVRInput.GetUp(OVRInput.Button.Back))
            {
                vrEventInfo.EventType = VrEventInfo.VrEventType.BackButton;
                OnBackButtonClickUp();
            }
#endif
            if (vrEventInfo.EventType == VrEventInfo.VrEventType.TouchpadClickDown)
            {
                touchdown = true;
                touchpadCentre.SetActive(touchdown);
            }
            else if (vrEventInfo.EventType == VrEventInfo.VrEventType.TouchpadClickUp)
            {
                touchpadCentre.SetActive(touchdown);
                touchdown = false;
            }

            if (flagTriggerRelease)
            {
                flagTriggerRelease = false;
                vrEventInfo.EventType = VrEventInfo.VrEventType.TriggerUp;
                vrEventInfo.GrabbedObject = null;
            }

            vrEventInfo.TriggerIsDown = IsTriggerDown();
            vrEventInfo.TouchIsDown = IsTouchpadDown();
            vrEventInfo.TouchIsTouched = IsTouchpadTouched();

#if UNITY_EDITOR
            if (!TouchIsDown) // handles fake touch drag using right MB
                vrEventInfo.TouchpadPosition = ConvertToTouchpadCoords(Input.mousePosition);
#else
            vrEventInfo.TouchpadPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
#endif
            ShowTouchpadFeedback(vrEventInfo.TouchpadPosition.y);

            if (readout.gameObject.activeSelf)
            {
                if (++periodicFrameCounter % periodicUiUpdate == 0)
                    readout.text = string.Format("y: {0:f}", vrEventInfo.TouchpadPosition.y);
            }

            if (currentPointerHandler)
                currentPointerHandler.Handle(vrEventInfo);

            ActivityManager.Instance.OnFrame(vrEventInfo);

            vrEventInfo.PrevTouchpadPosition = vrEventInfo.TouchpadPosition;

            UnitRotationZ = Utils.GetUnitRotationZ(vrEventInfo.ControllerRotation);

            if (paused)
            {
                unscaledTimer += Time.unscaledDeltaTime;
                if (unscaledTimer >= PAUSE_TIME)
                {
                    Debug.Log("Pause timed out");

                    unscaledTimer = 0f;
                    Time.timeScale = 1f;
                    paused = false;

                    if (backButtonPanel)
                        backButtonPanel.Show(false);
                }
            }
        }

        private void FixedUpdate()
        {
            FreeMove();
        }

        private void FreeMove()
        {
            if (freeMove && IsTouchpadDown())
            {
                Vector3 targetPos = Utils.GetOneAxisMovement(vrEventInfo);
                playerPhysics.MovePosition(targetPos);
                if (!moveGuide.activeSelf)
                    moveGuide.SetActive(true);
            }
            else if (moveGuide.activeSelf)
                moveGuide.SetActive(false);
        }

        /// <summary>
        /// I just can't get this to work reliably. The readings on 
        /// the touchpad, especially when it's clicked down,
        /// are inconsistent and inaccurate, sometimes reading
        /// y == 0.0 when it's well above the mid-mark (but
        /// not at the boundary of the sensor) in either the
        /// positive or negative direction. The click sensor,
        /// however, is always accurate and touch without 
        /// click is not too bad - but not perfect. Swiping
        /// is not such an issue as I'm reading differentials
        /// rather than absolute vertical coordinates.
        /// These issues are well-reported in the Oculus
        /// developer forum, without any interest by Oculus
        /// in fixing. General conclusion is that supporting use of
        /// the trackpad as a 4-way game controller is of
        /// no interest to Oculus, as the Go is not a game device.
        /// UPDATE: now the Go is not any sort of device.
        /// </summary>
        private void _deprecated_FreeMove()
        {
            bool showGuide = false;
            if (freeMove && touchdown)
            {
                Vector3 targetPos = Utils.GetOneAxisMovement(vrEventInfo);
                playerPhysics.MovePosition(targetPos);

                if (vrEventInfo.TouchpadPosition.y != 0)
                {
                    showGuide = true;

                    if (vrEventInfo.TouchpadPosition.y < 0f)
                        moveGuide.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                    else
                        moveGuide.transform.localRotation = Quaternion.identity;
                }
            }

            if (moveGuide.activeSelf && !showGuide)
                moveGuide.SetActive(false);
            else if (!moveGuide.activeSelf && showGuide)
                moveGuide.SetActive(true);
        }

        public bool RaycasterActive { get; set; }

        public EnPointerMode PointerMode
        {
            get => pointerMode;

            set
            {
                if (value != pointerMode)
                {
                    if (currentPointerHandler)
                        currentPointerHandler.Activate(false);

                    pointerMode = value;
                    switch (pointerMode)
                    {
                        case EnPointerMode.Pointing:
                            currentPointerHandler = beamPointerHandler;
                            break;
                        case EnPointerMode.Holding:
                            currentPointerHandler = grabPointerHandler;
                            break;
                        case EnPointerMode.None:
                            currentPointerHandler = nullPointerHandler;
                            break;
                    }

                    if (currentPointerHandler)
                        currentPointerHandler.Activate(true);
                }
            }
        }

        public void OverrideReticle(GameObject tempReticle, float scale = 1f)
        {
            currentPointerHandler.beamPointer.OverrideReticle(tempReticle, scale);
        }

        public void SetReticleHandler(System.Action overrideHandler)
        {
            currentPointerHandler.beamPointer.SetReticleHandler(overrideHandler);
        }

        public void RestoreReticle()
        {
            currentPointerHandler.beamPointer.RestoreReticle();
        }

        public void SimulateTriggerRelease()
        {
            flagTriggerRelease = true;
        }

        public void LockPlayer(bool locked, bool lockY = true)
        {
            playerPhysics.Sleep();
            freeMove = !locked;
            moveGuide.SetActive(freeMove);
            if (locked)
                playerPhysics.constraints = RigidbodyConstraints.FreezeAll;
            else
            {
                if (lockY)
                    playerPhysics.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                else
                    playerPhysics.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        public void SetGrabbedObject(Graspable g)
        {
            Graspable oldGrabbable = currentGrabbedObject;

            // Don't set if already set
            if (oldGrabbable != g)
            {
                if (oldGrabbable)
                    oldGrabbable.Clear();

                currentGrabbedObject = g;
            }
        }

        public Graspable GrabbedObject => currentGrabbedObject;

        public Vector3 ControllerPosition => lastControllerPosition;

        public Vector3 ControllerDirection => lastControllerDirection;

        public void SetHighlightedObject(Graspable g)
        {
            currentHighlightedObject = g;
        }

        public Graspable HighlightedObject => currentHighlightedObject;

        public void AddToSelectedGrabbables(Graspable grabbable)
        {
            if (!selectedGrabbables.Contains(grabbable))
                selectedGrabbables.Add(grabbable);
        }

        public List<Graspable> SelectedGrabbables => selectedGrabbables;

        public void ClearSelectedGrabbables()
        {
            if (selectedGrabbables.Count > 0)
            {
                foreach (Graspable g in selectedGrabbables)
                    g.Clear();

                selectedGrabbables.Clear();
            }
        }

        public void RemoveFromSelectedGrabbables(Graspable g)
        {
            int index = -1;
            for (int i = 0; i < selectedGrabbables.Count; i++)
            {
                if (selectedGrabbables[i] == g)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
                selectedGrabbables.RemoveAt(index);
        }

        public Transform CameraPos => cameraPos;

        public bool IsTriggerDown()
        {
#if UNITY_EDITOR

            return Input.GetMouseButton(0);
#else
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
#endif
        }

        public bool IsTouchpadDown()
        {
#if UNITY_EDITOR
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
#else
            return OVRInput.Get(OVRInput.Button.One);
#endif
        }

        public bool IsTouchpadTouched()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(1);
#else
            return OVRInput.Get(OVRInput.Touch.One);
#endif
        }

        public Vector2 GetTouchpadPosition()
        {
            return OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
        }

        /// <summary>
        /// Teleport but don't change Y coord. Because of
        /// Rift compatibility, OVRPlayerController doesn't
        /// let you set a fixed height offset for the camera.
        /// You can probably do it if you switch off positioning
        /// but then you don't get the virtual position of
        /// the Go controller (based on handledness etc).
        /// UPDATE: putting camera rig into transform with
        /// a permanent Y offset for Go
        /// </summary>
        /// <param name="newPos"></param>
        public void TeleportPlayerFlat(Vector3 newPos)
        {
            Vector3 pos = player.transform.position;
            pos.x = newPos.x;
            pos.z = newPos.z;
            player.transform.position = newPos;
        }

        public void TeleportPlayerAbsolute(Vector3 newPos)
        {
            player.transform.position = newPos;
        }

        public Transform Player => player;

        public Rigidbody PlayerRb => playerPhysics;

        public CapsuleCollider PlayerCollider => playerCollider;

        public void SetPlayerAspect(Transform t, bool forceCameraAlign = false)
        {
            player.Set(t);
            if (forceCameraAlign)
                AlignCamera();
        }

        public void AlignCamera()
        {
            OVRManager.display.RecenterPose();
        }

        public void ShowHud(bool showAvatar = true, bool showTime = false, bool showLife = false)
        {
            if (hud)
            {
                hud.gameObject.SetActive(showAvatar);
                hud.TimeIndicator(showTime);
                hud.LifeIndicator(showLife);
            }
        }

        public void SetHudAvatar(Sprite sprite)
        {
            if (hud)
                hud.SetAvatar(Instantiate(sprite) as Sprite);
        }

        public void SetHudLife(float unitVal)
        {
            if (hud)
                hud.SetDamage(unitVal);
        }

        public void SetHudTime(float unitVal, float seconds)
        {
            if (hud)
                hud.SetTime(unitVal, seconds);
        }

        public void PlayBgMusic(bool play, bool fadeout = true)
        {
            if (bgMusic)
            {
                if (play)
                    bgMusic.Play();
                else
                    bgMusic.Stop();
            }
        }

        public void PlayVoiceover(AudioClip clip, bool cue = false)
        {
            if (clip)
            {
                if (cue)
                    voiceover.Cue(clip);
                else
                    voiceover.Play(clip);
            }
        }

        public void DrawAdHocBeam(Vector3 startPoint, Vector3 endPoint)
        {
            beamPointerHandler.beamPointer.DrawImmediate(startPoint, endPoint);
        }

        public void ClickGrabSound()
        {
            if (clickGrab)
                LeanAudio.play(clickGrab, Vector3.zero, 0.15f);
        }

        public void ClickReleaseSound()
        {
            if (clickRelease)
                LeanAudio.play(clickRelease, Vector3.zero, 0.15f);
        }

        private Vector2 ConvertToTouchpadCoords(Vector3 mousePos)
        {
            float halfWidth = Screen.width / 2f;
            float halfHeight = Screen.height / 2f;

            float x = mousePos.x - halfWidth;
            float y = mousePos.y - halfHeight;

            return new Vector2(x / halfWidth, y / halfHeight);
        }

        private void ShowTouchpadFeedback(float yPos)
        {
            if (yPos != 0f)
            {
                if (yPos > 0f)
                {
                    if (!touchpadNorth.activeSelf)
                        touchpadNorth.SetActive(true);
                    if (touchpadSouth.activeSelf)
                        touchpadSouth.SetActive(false);
                }
                else if (yPos < 0f)
                {
                    if (touchpadNorth.activeSelf)
                        touchpadNorth.SetActive(false);
                    if (!touchpadSouth.activeSelf)
                        touchpadSouth.SetActive(true);
                }
            }
            else
            {
                if (touchpadNorth.activeSelf)
                    touchpadNorth.SetActive(false);
                if (touchpadSouth.activeSelf)
                    touchpadSouth.SetActive(false);
            }
        }

        private bool OnBackButtonClickUp()
        {
            Debug.Log("OnBackButtonClickUp");

            if (!paused)
            {
                Time.timeScale = 0f;
                paused = true;
                unscaledTimer = 0f;
                if (backButtonPanel)
                    backButtonPanel.Show().PlaceClose();
            }
            else
            {
                Debug.Log("Return to menu confirmed");

                unscaledTimer = 0f;
                paused = false;
                Time.timeScale = 1f;

                QuiteSensible.TrackPlayer trackPlayer = GameObject.FindObjectOfType<QuiteSensible.TrackPlayer>();
                if (trackPlayer)
                {
                    trackPlayer.StopPlay();
                    Destroy(trackPlayer.gameObject);
                }

                StopAllCoroutines();
                LeanTween.cancelAll();
                ActivityManager.Instance.ReturnToMenuImmediate(); // No fade or cleanup
            }
            return false;
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QS
{
    /// <summary>
    /// Async handler for all interactions.
    /// WIP: removing Oculus dependencies
    /// and leaving enough input code for 
    /// general testing of modules.
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

        private const float MousewheelRotationCoeff = 20f;
        private const int periodicUiUpdate = 20;

        [System.NonSerialized]
        public PointerHandler currentPointerHandler;

        public enum EnPointerMode { None, Pointing, Holding, Ui, LockedObject, CustomBeam };

        private EnPointerMode pointerMode = EnPointerMode.None;
        private Camera mainCam;
        private VrEventInfo vrEventInfo = new VrEventInfo();
        private Vector3 lastControllerPosition, lastControllerDirection; // For quiclk queries
        private Vector3 calcOffset = Vector3.zero;
        private bool flagTriggerRelease;
        private bool freeMove;
        private Quaternion globalPointerRot;
        private bool paused;
        private float unscaledTimer;
        private float simRotationAngle;

        private int periodicFrameCounter;
        private bool touchdown; // Monitor via clicks

        private const float PAUSE_TIME = 5f;

        // Clamped controller z axis twist
        public float UnitRotationZ { get; private set; }
        // Runtime checks by stuff like car sim
        public bool TriggerIsDown => IsTriggerDown();
        public bool TouchIsDown => IsTouchpadDown();
        public bool TouchIsTouched => IsTouchpadTouched();

        private void Start()
        {
            mainCam = Camera.main;
            PlayerRb = player.GetComponent<Rigidbody>();
            if (PlayerRb)
                LockPlayer(true);
            PlayerCollider = player.GetComponent<CapsuleCollider>();
            CameraPos = player.Find("CenterEyeAnchor");
            if (!CameraPos)
                CameraPos = mainCam.transform;
            PointerMode = EnPointerMode.Pointing;

            Debug.Log("Device present? " + UnityEngine.XR.XRDevice.isPresent);
            Debug.Log("VR enabled? " + UnityEngine.XR.XRSettings.enabled);

            ActivitySettings.Asset.ResetCurrentExperienceScores();

            if (readout && !ActivitySettings.Asset.showTouchpadY)
                readout.gameObject.SetActive(false);
        }

        private void Update()
        {
            bool connected = false;
            Vector3 pointerDir = Vector3.zero, angularAcceleration = Vector3.zero;
            Vector3 globalPointerPos = Vector3.zero;


            vrEventInfo.ClearTemporal();

            connected = false;

            calcOffset = mainCam.transform.right * controllerOffsetDesktop.x + mainCam.transform.up *
                controllerOffsetDesktop.y + mainCam.transform.forward * controllerOffsetDesktop.z;

            float deltaMouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            simRotationAngle += deltaMouseScrollWheel * MousewheelRotationCoeff;
            Vector3 camAngles = Camera.main.transform.rotation.eulerAngles;
            camAngles.z = simRotationAngle;
            globalPointerRot = Quaternion.Euler(camAngles);

            globalPointerPos = mainCam.transform.position + calcOffset;
            if (mainCam.transform.parent)
                pointerDir = mainCam.transform.parent.TransformDirection(mainCam.transform.localRotation * Vector3.forward);
            else
                pointerDir = mainCam.transform.localRotation * Vector3.forward;
            angularAcceleration = Vector3.zero;

            if (vrEventInfo.Connected)
            {
                vrEventInfo.Connected = false;
                vrEventInfo.Connected = false;
            }

            if (connected && !vrEventInfo.Connected)
                vrEventInfo.Connected = true;

            lastControllerPosition = globalPointerPos;
            lastControllerDirection = pointerDir;

            vrEventInfo.ControllerPosition = globalPointerPos;
            vrEventInfo.ControllerRotation = globalPointerRot;
            vrEventInfo.ControllerDirection = pointerDir;
            vrEventInfo.AngularAccelleration = angularAcceleration;

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

            if (vrEventInfo.EventType == VrEventInfo.VrEventType.TouchpadClickDown)
            {
                touchdown = true;
                if (touchpadCentre) touchpadCentre.SetActive(touchdown);
            }
            else if (vrEventInfo.EventType == VrEventInfo.VrEventType.TouchpadClickUp)
            {
                if (touchpadCentre) touchpadCentre.SetActive(touchdown);
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

            if (!TouchIsDown) // handles fake touch drag using right MB
                vrEventInfo.TouchpadPosition = ConvertToTouchpadCoords(Input.mousePosition);
            if (readout && readout.gameObject.activeSelf)
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
                PlayerRb.MovePosition(targetPos);
                if (moveGuide && !moveGuide.activeSelf)
                    moveGuide.SetActive(true);
            }
            else if (moveGuide && moveGuide.activeSelf)
                moveGuide.SetActive(false);
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
            PlayerRb.Sleep();
            freeMove = !locked;
            if (moveGuide)
                moveGuide.SetActive(freeMove);
            if (locked)
                PlayerRb.constraints = RigidbodyConstraints.FreezeAll;
            else
            {
                if (lockY)
                    PlayerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                else
                    PlayerRb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        public void SetGrabbedObject(Graspable g)
        {
            Graspable oldGrabbable = GrabbedObject;

            // Don't set if already set
            if (oldGrabbable != g)
            {
                if (oldGrabbable)
                    oldGrabbable.Clear();

                GrabbedObject = g;
            }
        }

        public Graspable GrabbedObject { get; private set; }

        public Vector3 ControllerPosition => lastControllerPosition;

        public Vector3 ControllerDirection => lastControllerDirection;

        public void SetHighlightedObject(Graspable g)
        {
            HighlightedObject = g;
        }

        public Graspable HighlightedObject { get; private set; }

        public void AddToSelectedGrabbables(Graspable grabbable)
        {
            if (!SelectedGrabbables.Contains(grabbable))
                SelectedGrabbables.Add(grabbable);
        }

        public List<Graspable> SelectedGrabbables { get; } = new List<Graspable>();

        public void ClearSelectedGrabbables()
        {
            if (SelectedGrabbables.Count > 0)
            {
                foreach (Graspable g in SelectedGrabbables)
                    g.Clear();

                SelectedGrabbables.Clear();
            }
        }

        public void RemoveFromSelectedGrabbables(Graspable g)
        {
            int index = -1;
            for (int i = 0; i < SelectedGrabbables.Count; i++)
            {
                if (SelectedGrabbables[i] == g)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
                SelectedGrabbables.RemoveAt(index);
        }

        public Transform CameraPos { get; private set; }

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
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        }

        public bool IsTouchpadTouched()
        {
            return Input.GetMouseButton(1);
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

        public Rigidbody PlayerRb { get; private set; }

        public CapsuleCollider PlayerCollider { get; private set; }

        public void SetPlayerAspect(Transform t, bool forceCameraAlign = false)
        {
            player.Set(t);
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

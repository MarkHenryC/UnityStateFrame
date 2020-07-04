using UnityEngine;

namespace QS
{
    public abstract class StateProcessor : MonoBehaviour
    {
        public Transform playerInitialAspect;
        [Tooltip("If non-zero, this is a timed activity")]
        public float timeToCompleteActivity;
        public GameObject stateObjects;
        public GameObject[] sharedStateObjects;
        [Tooltip("Uses such as excluding indoor lighting from an outdoor scene")]
        public GameObject[] excludedStateVisuals;
        [Tooltip("Allow free navigation with physics")]
        public bool freeMovement;
        [Tooltip("Allow gravity with physics")]
        public bool useGravity;
        [Tooltip("Exclude this state from the current activity flow")]
        public bool exclude;
        [Tooltip("Optional: voiceover or any sound to play at start of state")]
        public AudioClip introSound;
        [Tooltip("Optional: when player clicks, state exits and continues to next")]
        public bool clickToContinue;

        protected ActivityBase currentActivity;
        protected float slowFrameInterval;
        protected float slowFrameCounter;
        protected bool exitPosted, exitInitiated, exitFadeout;
        protected float timeRemaining;
        protected bool timerRunning, timerDisabled, timerPaused;
        protected System.Type nextState;

        public int ActivityScore
        {
            get { return currentActivity.activityScore; }
        }

        public int ActivityBonus
        {
            get { return currentActivity.activityBonusPoints; }
        }

        public string ActivityReport
        {
            get { return currentActivity.GetActivityReport(); }
            set { currentActivity.SetActivityReport(value); }
        }

        /// <summary>
        /// Set current activity data from the StateProcessor-derived class. 
        /// The activity will then do a once-only update when it finishes via Finish()
        /// </summary>
        /// <param name="score"></param>
        /// <param name="bonus"></param>
        /// <param name="potential"></param>
        protected void UpdateScore(int score, int bonus, int potential = ActivitySettings.pointsPerChallenge)
        {
            Debug.Log("Calling Score() from StateProcessor");

            currentActivity.activityScore = score;
            currentActivity.activityBonusPoints = bonus;
            currentActivity.activityPotential = potential;
        }

        protected void IntSet(string tag, int val)
        {
            currentActivity.SetInt(tag, val);
        }

        protected void FloatSet(string tag, float val)
        {
            currentActivity.SetFloat(tag, val);
        }

        protected void StringSet(string tag, string val)
        {
            currentActivity.SetString(tag, val);
        }

        protected bool IntVal(string tag, out int val)
        {
            return currentActivity.GetInt(tag, out val);
        }

        protected bool FloatVal(string tag, out float val)
        {
            return currentActivity.GetFloat(tag, out val);
        }

        protected bool StringVal(string tag, out string val)
        {
            return currentActivity.GetString(tag, out val);
        }

        public virtual void Enter(ActivityBase associatedActivity, StateProcessor previousState = null)
        {
            if (previousState != null)
                previousState.Exit();

            gameObject.SetActive(true);

            currentActivity = associatedActivity;

            timeRemaining = timeToCompleteActivity;

            if (playerInitialAspect)
                ControllerInput.Instance.SetPlayerAspect(playerInitialAspect);

            if (sharedStateObjects != null)
            {
                foreach (GameObject so in sharedStateObjects)
                    if (so)
                        so.SetActive(true);
            }

            slowFrameInterval = ActivitySettings.Asset.slowFrameInterval; // Cache instead of hitting every frame

            ShowVisuals(true);
            ControllerInput.Instance.LockPlayer(!freeMovement, !useGravity);

            if (excludedStateVisuals.Length > 0)
            {
                foreach (GameObject g in excludedStateVisuals)
                    if (g)
                        g.SetActive(false);
            }

            timerRunning = false;
            timerDisabled = false;

            if (introSound)
                ControllerInput.Instance.PlayVoiceover(introSound);

            Debug.Log("Entering State " + GetType().Name + " for Activity " + currentActivity.name);
        }

        public virtual void Suspend()
        {
            Debug.Log("Suspend state " + GetType().Name);
        }

        public virtual void Exit()
        {
            ShowVisuals(false);
            gameObject.SetActive(false);
            if (sharedStateObjects != null)
            {
                foreach (GameObject so in sharedStateObjects)
                    if (so)
                        so.SetActive(false);
            }
            exitPosted = false;
            exitInitiated = false;
            exitFadeout = false;

            ControllerInput.Instance.ShowHud(true); // For when task completes before timeout

            if (excludedStateVisuals.Length > 0)
            {
                foreach (GameObject g in excludedStateVisuals)
                    if (g)
                        g.SetActive(true); // these are by default on
            }

            Debug.Log("Exiting state " + GetType().Name);
        }

        public virtual void ShowVisuals(bool show)
        {
            if (stateObjects)
                stateObjects.SetActive(show);
        }

        public virtual void ProcessGrabbedObject(VrEventInfo processedVrEventInfo)
        {
            if (processedVrEventInfo.GrabbedObject)
            {
                processedVrEventInfo.GrabbedObject.UpdateRotation(processedVrEventInfo.ControllerRotation);

                if (processedVrEventInfo.TouchIsTouched)
                    PushPull(processedVrEventInfo);

                Vector3 newPos = processedVrEventInfo.PointerTarget;

                processedVrEventInfo.GrabbedObject.MoveTo(newPos);
                processedVrEventInfo.HoldingTargetPosition = newPos;
            }
        }

        public virtual void PushPull(VrEventInfo processedVrEventInfo)
        {
            if (!processedVrEventInfo.TouchIsDown)
            {
                processedVrEventInfo.BeamDistance += (processedVrEventInfo.TouchpadMoved.y * Time.deltaTime * ActivitySettings.Asset.beamExtensionSpeed);
                processedVrEventInfo.BeamDistance = Mathf.Clamp(processedVrEventInfo.BeamDistance, ActivitySettings.Asset.minBeamLength, ActivitySettings.Asset.maxBeamLength);
            }
        }

        public virtual void OnFrame(VrEventInfo processedVrEventInfo)
        {
            if (timerRunning && !timerDisabled && !timerPaused)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0f)
                    OnTimedOut();
            }

            if (exitPosted && !exitInitiated)
            {
                exitInitiated = true;
                if (exitFadeout)
                {
                    if (nextState != null)
                        ActivityManager.Instance.FadeOutThen(() => { currentActivity.SetState(nextState); });
                    else
                        ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(); });
                }
                else
                {
                    if (nextState != null)
                        ActivityManager.Instance.SetActivity(nextState);
                    else
                        ActivityManager.Instance.Next();
                }
            }
            else
            {
                switch (processedVrEventInfo.EventType)
                {
                    case VrEventInfo.VrEventType.TriggerDown:
                        if (clickToContinue)
                        {
                            clickToContinue = false;
                            PostExit(true);
                        }
                        else
                        {
                            if (processedVrEventInfo.GrabbedObject)
                            {
                                ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Holding;
                                // BeamDistance is set in raycaster and an offset is stored in grabbed object.
                            }

                            if (processedVrEventInfo.GrabbedRb)
                                processedVrEventInfo.GrabbedRb.useGravity = false;
                        }
                        break;
                    case VrEventInfo.VrEventType.Teleport:
                        ControllerInput.Instance.TeleportPlayerFlat(processedVrEventInfo.HitObject.transform.position);
                        Teleportable tele = processedVrEventInfo.HitObject.GetComponent<Teleportable>();
                        Debug.Assert(tele, "Somehow got teleport event but no teleportable component");
                        tele.OnTeleport?.Invoke();
                        break;
                    case VrEventInfo.VrEventType.TriggerUp:
                        if (processedVrEventInfo.GrabbedObject)
                            processedVrEventInfo.GrabbedObject = null;

                        // Don't revert to pointing if it's a hidden (None) control beam
                        if (ControllerInput.Instance.PointerMode != ControllerInput.EnPointerMode.None)
                            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Pointing;
                        break;
                }
                ProcessGrabbedObject(processedVrEventInfo);

                slowFrameCounter += Time.deltaTime;
                if (slowFrameCounter >= slowFrameInterval)
                    OnSlowFrame(processedVrEventInfo);
            }
        }

        public virtual void OnSlowFrame(VrEventInfo processedVrEventInfo)
        {
            if (timerRunning)
                ControllerInput.Instance.SetHudTime(timeRemaining / timeToCompleteActivity, timeRemaining);
        }

        protected virtual void OnTimedOut()
        {
            ControllerInput.Instance.ShowHud(true); // only avatar
            timerRunning = false;
        }

        protected virtual void PauseTimer(bool pause)
        {
            timerPaused = pause;
        }

        protected virtual void DisableTimer()
        {
            ControllerInput.Instance.ShowHud(true); // only avatar
            timerDisabled = true;
        }

        protected void StartTimer()
        {
            Debug.Assert(timeRemaining > 0f, "StartTimer called but time is zero");

            if (!timerRunning && !timerDisabled)
            {
                ControllerInput.Instance.ShowHud(true, true);
                ControllerInput.Instance.SetHudLife(1f); // For when it's reused in case of an update delay
                timerRunning = true;
            }
        }

        protected void PostExit(bool fadeout = false, System.Type type = null)
        {
            exitPosted = true;
            exitFadeout = fadeout;
            nextState = type;
        }

        protected void PlayReward()
        {
            ActivitySettings.Asset.PlayReward();
        }
    }
}
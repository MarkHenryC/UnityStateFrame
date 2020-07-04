using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace QS
{
    public class ActivityManager : Singleton<ActivityManager>
    {        
        public ActivityBase[] activities;
        public SceneLoader sceneLoader;
        public GameObject commonVisuals; // Need to switch this off to prevent flashing on scene change
        
        [Header("If non-null, overrides numeric sceneindex. No need to include 'Activity' prefix")]
        [HideInInspector]
        public string startActivityName;

        public bool ActivitiesComplete { get; set; } // So we don't try to re-load after completing the sequence

        private ActivityBase currentActivity;
        private int startActivityIndex;

        private const string START_SCENE = "Start";

        protected override void Awake()
        {
            base.Awake();

            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
            OVRManager.display.displayFrequency = 72.0f;

            activities = Utils.GetNonExcludedActivities<ActivityBase>(transform);

            ClearVisuals();            

#if UNITY_EDITOR

            if (startActivityName.Usable())
            {
                Transform startT = transform.Find("Activity" + startActivityName) ?? transform.Find(startActivityName);
                if (startT)
                {
                    for (int i = 1; i < activities.Length; i++)
                    {
                        if (activities[i].transform == startT)
                        {
                            startActivityIndex = i;
                            break;
                        }
                    }
                }
            }

            // Editor settings override json in desktop
            ActivitySettings.Asset.masterNavOverride = ActivitySettings.Asset.navigationOverride;
            ActivitySettings.Asset.startActivityIndex = startActivityIndex;
#else
            // Always disable for device playback as we don't want inter-state jumps, only activity-level
            ActivitySettings.Asset.navigationOverride = false;
#endif
            // Keypad at runtime overrides all
            if (ActivitySettings.Asset.overrideCode > 0)
            {
                if (ActivitySettings.Asset.overrideCode >= 9900)
                {
                    // 99nn is control code
                    // 9910 show touchpad Y
                    switch (ActivitySettings.Asset.overrideCode)
                    {
                        case 9910:
                            ActivitySettings.Asset.showTouchpadY = true;
                            break;
                    }
                }
                else // quick scene jump
                {
                    ActivitySettings.Asset.masterNavOverride = true;
                    ActivitySettings.Asset.startActivityIndex = ActivitySettings.Asset.overrideCode;                    
                }
                ActivitySettings.Asset.overrideCode = 0;
            }
        }

        public void ClearVisuals()
        {
            activities = Utils.GetNonExcludedActivities<ActivityBase>(transform);

            foreach (var a in activities)
                a.SetVisuals();
        }

        public void FadeOutThen(Action a)
        {
            if (ControllerInput.Instance.screenFade)
                ControllerInput.Instance.screenFade.FadeOutThen(a);
            else
            {
                Debug.LogWarning("No fader in FadeOutThen call");
                if (a != null)
                    a();
                else
                    Debug.LogWarning("No action in FadeOutThen");
            }
        }

        public ActivityBase CurrentActivity
        {
            get
            {
                if (ActivitiesComplete || activities == null || activities.Length == 0)
                    return null;

                if (!currentActivity)
                {
                    if (ActivitySettings.Asset.masterNavOverride &&
                        activities.Length > ActivitySettings.Asset.startActivityIndex &&
                        ActivitySettings.Asset.startActivityIndex >= 0)
                    {
                        Debug.LogFormat("Override set. Starting at scene {0}", ActivitySettings.Asset.startActivityIndex);

                        currentActivity = activities[ActivitySettings.Asset.startActivityIndex];
                        ActivitySettings.Asset.startActivityIndex = 0; // Reset
                        ActivitySettings.Asset.masterNavOverride = false;  // Reset
                    }
                    else
                    {
                        Debug.Log("No activity set. Trying first in list");

                        currentActivity = activities[0];
                    }

                    if (currentActivity)
                        currentActivity.Initialize();
                }

                return currentActivity;
            }
        }

        public void SetActivity(ActivityBase activity, bool finishCurrent = true)
        {
            if (finishCurrent && currentActivity != null)
            {
                Debug.Log("Finishing current activity: " + currentActivity.GetType().Name);

                currentActivity.Finish();
                currentActivity = null;
            }

            currentActivity = activity;

            if (currentActivity)
                currentActivity.Initialize();
        }

        /// <summary>
        /// Pass name of ActivityBase-derived class.
        /// Default to finishing current activity even
        /// if we don't find the new one.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="finishCurrent"></param>
        public void SetActivity(string name, bool finishCurrent = true)
        {
            if (finishCurrent && currentActivity != null)
            {
                Debug.Log("Finishing current activity: " + currentActivity.GetType().Name);

                currentActivity.Finish();
                currentActivity = null;
            }

            for (int i = 0; i < activities.Length; i++)
            {
                if (activities[i].GetType().Name == name)
                {
                    Debug.Log("Starting new activity: " + name);

                    currentActivity = activities[i];
                    currentActivity.Initialize();

                    break;
                }
            }

            if (!currentActivity)
                Debug.LogWarning("Activity not found: " + name);
        }

        public void SetActivity(System.Type type, bool finishCurrent = true)
        {
            if (finishCurrent && currentActivity != null)
            {
                Debug.Log("Finishing current activity: " + currentActivity.GetType().Name);

                currentActivity.Finish();
                currentActivity = null;
            }

            currentActivity = activities.SingleOrDefault(s => s.GetType() == type);
            Debug.Assert(currentActivity, "No activity found that matches type " + type);

            if (currentActivity)
                currentActivity.Initialize();
        }

        /// <summary>
        /// A generic next mechanism that can be called by
        /// UnityEvents. It defaults to nex state in 
        /// current activity or next activity if states are complete.
        /// This is a handy default behaviour but of course in 
        /// some states the next state will be conditional and
        /// has to be done in code. If nextActivity is true
        /// then any more states in this activity are skipped
        /// </summary>
        public void Next(bool nextActivity = false)
        {
            if (activities == null)
            {
                Debug.Log("No activities to process");                
                return;
            }

            if (currentActivity == null && activities.Length >= 0)
                SetActivity(activities[0]);
            else
            {
                for (int i = 0; i < activities.Length; i++)
                {
                    if (activities[i] == currentActivity)
                    {
                        if (nextActivity || !currentActivity.Next())
                        {
                            if ((i + 1) < activities.Length)
                            {
                                SetActivity(activities[i + 1]);
                                return;
                            }
                            else
                            {
                                ReturnToMenu();
                            }
                        }
                    }
                }
            }

            if (currentActivity)
                Debug.LogWarning("Current activity active but not in list: " + currentActivity.name);
        }

        public void ReturnToMenu(bool fade = true)
        {
            SetActivity((ActivityBase)null);
            ActivitiesComplete = true;
            sceneLoader.LoadScene(START_SCENE, fade);
            commonVisuals.SetActive(false);
        }

        public void ReturnToMenuImmediate()
        {
            SetActivity((ActivityBase)null, false);
            ActivitiesComplete = true;
            sceneLoader.LoadScene(START_SCENE, false);
            commonVisuals.SetActive(false);
        }

        /// <summary>
        /// This message may come directly from ControllerInput
        /// or may be processed by an intermediary
        /// </summary>
        /// <param name="info"></param>
        public void OnFrame(VrEventInfo processedVrEventInfo)
        {
            if (CurrentActivity)
                CurrentActivity.OnFrame(processedVrEventInfo);
        }
    }
}
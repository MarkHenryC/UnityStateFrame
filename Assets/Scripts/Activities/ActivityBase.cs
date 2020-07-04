using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace QS
{
    public class ActivityBase : MonoBehaviour
    {
        [Tooltip("Group all the visuals for this activity under the specified hierarcy folder. They will be enabled when the activity starts")]
        public GameObject visuals;
        [Tooltip("Visuals that might be shared with other activities. They will be enabled/disabled when the activity starts/stops")]
        public GameObject[] sharedVisuals;
        [Tooltip("Handy for things like the video player, which has to switch off the entire graphics environment")]
        public GameObject[] excludedVisuals;

        [Header("For testing")]
        [Tooltip("Jump to a specific state index (navigation bypass must be enabled in ActivityManager)")]
        public int startActivityStateIndex;
        [Tooltip("Do not process this activity at runtime")]
        public bool exclude;

        [Tooltip("Only used in later versions. If non-zero, will cause totals to be added on activity exit")]
        public int activityScore;
        [Tooltip("Only used in later versions. Goes with activityPoints")]
        public int activityBonusPoints;
        [Tooltip("Only used in later versions. The total possible points in this activity")]
        public int activityPotential;

        [Tooltip("Drag StateProcessor-derived classes here. Ensure the name is correct when referenced in the specific activity")]
        protected StateProcessor[] states;
        protected StateProcessor currentState;

        protected Dictionary<string, int> intDict = new Dictionary<string, int>();
        protected Dictionary<string, float> flDict = new Dictionary<string, float>();
        protected Dictionary<string, string> strDict = new Dictionary<string, string>();

        protected string activityReport;

        public void SetInt(string tag, int val)
        {
            intDict[tag] = val;
        }

        public void SetFloat(string tag, float val)
        {
            flDict[tag] = val;
        }

        public void SetString(string tag, string val)
        {
            strDict[tag] = val;
        }

        public bool GetInt(string tag, out int val)
        {
            val = default;
            bool rslt = intDict.ContainsKey(tag);
            if (rslt)
                val = intDict[tag];

            if (!rslt)
                Debug.LogError("Failed to get int for key " + tag);

            return rslt;
        }

        public bool GetFloat(string tag, out float val)
        {
            val = default;
            bool rslt = intDict.ContainsKey(tag);
            if (rslt)
                val = intDict[tag];

            if (!rslt)
                Debug.LogError("Failed to get float for key " + tag);

            return rslt;
        }

        public bool GetString(string tag, out string val)
        {
            val = default;
            bool rslt = strDict.ContainsKey(tag);
            if (rslt)
                val = strDict[tag];

            if (!rslt)
                Debug.LogError("Failed to get string for key " + tag);

            return rslt;
        }

        public void SetActivityReport(string report)
        {
            activityReport = report;
        }

        public string GetActivityReport()
        {
            return activityReport;
        }

        public T GetState<T>(string obName) where T : StateProcessor
        {
            return (T)states.SingleOrDefault(state => state.name == obName);
        }

        public virtual void SetVisuals()
        {
            states = Utils.GetNonExcludedStates<StateProcessor>(transform);
            HideVisuals();
        }

        /// <summary>
        /// Activity is about to start
        /// </summary>
        public virtual void Initialize()
        {
            Debug.Log("Starting activity " + GetType().Name);

            SetVisuals(true);

            if (excludedVisuals.Length > 0)
            {
                foreach (GameObject g in excludedVisuals)
                    if (g)
                        g.SetActive(false);
            }

            gameObject.SetActive(true);

            ActivitySettings.Asset.currentActivityScore = 0;
            ActivitySettings.Asset.currentActivityBonusPoints = 0;

            // If master overrides (via json config) don't allow inter-state jumps that were set up
            // for editor testing otherwise it would get confusing.
            if (!ActivitySettings.Asset.navigationOverride)
                First();
            else
                SetState();
        }

        /// <summary>
        /// Just an initialisation routine
        /// as it's easy to leave things
        /// on in the editor while testing
        /// </summary>
        public virtual void HideVisuals()
        {
            foreach (var s in states)
            {
                s.ShowVisuals(false);
                s.gameObject.SetActive(false);
            }

            SetVisuals(false);
        }

        /// <summary>
        /// Forwarded by ActivityManager to the current
        /// activity
        /// </summary>
        /// <param name="info"></param>
        public virtual void OnFrame(VrEventInfo processedVrEventInfo)
        {
            if (currentState)
                currentState.OnFrame(processedVrEventInfo);
        }

        /// <summary>
        /// This activity is about to end
        /// </summary>
        public virtual void Finish()
        {
            Debug.Log("Ending activity " + GetType().Name);

            SetVisuals(false);

            if (excludedVisuals.Length > 0)
            {
                foreach (GameObject g in excludedVisuals)
                    if (g)
                        g.SetActive(true); // these are by default on
            }

            if (currentState)
            {
                currentState.Exit();
                currentState = null;
            }

            if (activityPotential  > 0)
            {
                Debug.Log("Updating global experience scores in ActivityBase.Finish()");

                ActivitySettings.Asset.currentExperienceTotalScore += activityScore;
                ActivitySettings.Asset.currentExperienceBonusPoints += activityBonusPoints;
                ActivitySettings.Asset.currentExperienceMaxValue += activityPotential;
            }

            gameObject.SetActive(false);
        }

        public virtual void SetState(StateProcessor newState)
        {
            StateProcessor prevState = currentState;
            currentState = newState;
            if (currentState)
                currentState.Enter(this, prevState);
        }

        public virtual void SetState(string name)
        {                        
            StateProcessor prevState = currentState;

            currentState = states.SingleOrDefault(s => s.name == name);

            Debug.Assert(currentState, "No state object found for component " + name);

            if (currentState)
                currentState.Enter(this, prevState);
        }

        public virtual void SetState(System.Type type)
        {
            StateProcessor prevState = currentState;

            currentState = states.SingleOrDefault(s => s.GetType() == type);

            Debug.Assert(currentState, "No state object found for component " + type);

            if (currentState)
                currentState.Enter(this, prevState);
        }

        /// <summary>
        /// Auto set state when test flag is set in ActivitySettings
        /// </summary>
        public virtual void SetState()
        {
            StateProcessor prevState = currentState;

            if (startActivityStateIndex < states.Length)
                currentState = states[startActivityStateIndex];

            Debug.Assert(currentState, "No state object found for state index " + startActivityStateIndex);

            if (currentState)
                currentState.Enter(this, prevState);
            else
                ActivityManager.Instance.Next();
        }

        /// <summary>
        /// Next state in list managed by currentActivity.
        /// If no more states, flag as unhandled
        /// </summary>
        public virtual bool Next()
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] == currentState)
                {
                    if ((i + 1) < states.Length)
                        SetState(states[i + 1]);
                    else
                    {
                        if (currentState != null)
                        {
                            currentState.Exit();
                            currentState = null;
                        }
                    }
                    break;
                }
            }

            return currentState != null;
        }

        /// <summary>
        /// For default behaviour
        /// </summary>
        public virtual void First()
        {
            if (states.Length > 0)
                SetState(states[0]);
            else if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }
        }

        public void SetVisuals(bool set)
        {
            if (visuals)
                visuals.SetActive(set);

            if (sharedVisuals.Length > 0)
            {
                foreach (GameObject g in sharedVisuals)
                    if (g)
                        g.SetActive(set);
            }

            if (excludedVisuals.Length > 0)
            {
                foreach (GameObject g in excludedVisuals)
                    if (g)
                        g.SetActive(!set);
            }
        }
    }
}
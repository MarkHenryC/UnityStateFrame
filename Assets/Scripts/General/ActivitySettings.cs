using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    [CreateAssetMenu(fileName = "ActivitySettings", menuName = "QS/ActivitySettings", order = 2)]
    public class ActivitySettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("System")]

        [Tooltip("Enable jumping straight to activity and state for testing")]
        public bool navigationOverride;
        [Tooltip("Master setting which ignores runtime status")]
        public bool masterNavOverride;
        [Tooltip("Overrides the ActivityManager setting if set in json")]
        public int startActivityIndex;
        [Tooltip("Set from loadable json")]
        public bool AllowSkipScenes;
        [Tooltip("Set timeout for info panels which auto-continue")]
        public float infoDisplayTime = 5f;
        public float shortDisplayTime = 3f;
        [Tooltip("Set timeout for title screens which auto-continue")]
        public float titleDisplayTime = 7.5f;
        [Tooltip("Time to auto-advance when no interactions")]
        public float interactionGapTimeout = 1.5f;
        [Tooltip("One unit represents chunk of text to measure display time by")]
        public float infoLengthUnit = 100f;
        [Tooltip("For intro scene video load. Includes extension, which must be .mkv")]
        public string defaultVideoName;
        [Tooltip("Store any overrides from json file here")]
        public Overrides overrides;
        [Tooltip("For testing specific high-volume debug output")]
        public bool masterDebugFlag;
        [Tooltip("Show the reticle even when not over a grabbable")]
        public bool showReticleAlways;
        [Tooltip("Display touchpad Y reading on HUD")]
        [System.NonSerialized] // Can be set on virtual pad in start, so don't save setting
        public bool showTouchpadY;

        [Header("Scoring")]

        public const int maxHealth = 100;
        public const int pointsPerChallenge = 25;

        [Tooltip("Percent health for current game")]
        public int currentHealth;

        [Header("General")]

        [Tooltip("Prevent pointing at teleport below. As dot product of pointer and up vector")]
        public float minimumAngleForTeleport = -0.95f;
        [Tooltip("How far we place a button in front of player")]
        public float buttonFromCameraDistance = 2f;
        [Tooltip("How far we place an info panel in front of player")]
        public float infoPanelFromCameraDistance = 2.1f;
        [Tooltip("How far we place a close-up panel in front of player")]
        public float closeupPanelFromCameraDistance = .75f;
        [Tooltip("How far we move the panel up fron the default button position")]
        public float infoPanelYOffset = 1f;
        [Tooltip("For frames that don't need to be checked as often like UI")]
        public float slowFrameInterval = 0.5f; // Twice per second
        [Tooltip("If testing we can manually set start activity")]
        public bool testing;
        [Tooltip("for animating an object on a table to return to its original position")]
        public float localReturnTime = 1f;
        [Tooltip("General pointing raycast distance. Default 20f")]
        public float raycastDistance = 20f;
        [Tooltip("Speed of extension movement of beam - swiping vertically on touchpad - on grabbed object. Default 100")]
        public float beamExtensionSpeed = 100f;
        [Tooltip("Speed of rotation of attached object. Default 1")]
        public float objectRotationSpeed = 36f;
        [Tooltip("So we don't process changes around the centre. Default 0.25. For horizontal touchpad moves rotating object")]
        public float touchpadXDeadspot = 0.25f;
        [Tooltip("For picking things up and dragging toward you. Don't crash into camera. Default 1f")]
        public float minBeamLength = 1f;
        [Tooltip("For picking things up and pushing away from you. Set a limit. Default 19f, just below raycast distance")]
        public float maxBeamLength = 19f;
        [Tooltip("Choose a layer for raycasting. This is second priority to any UI layer raycasts")]
        public LayerMask interactableTarget;
        [Tooltip("To set reticle back slightly from target as a scale of distance 1. 0.99 is default")]
        public float inset;
        [Tooltip("The sound to play when the player gets all correct before timeout")]
        public AudioClip rewardSound;
        [Tooltip("Override parameters via the Start activity menu. Takes precedence over the override json file")]
        public int overrideCode;
        [Tooltip("Controller rotation damping")]
        public int lowPassSamples;
        [Tooltip("Move speed")]
        public float playerSpeed = .01f;
        [Tooltip("Damage limit")]
        public float damageCapacity = 13f;

        // May not need the mp4
        public string WIN_VIDEO_EXT = ".mp4";
        public string GO_VIDEO_EXT = ".mkv";

        [Header("For choosing a self avatar or icon")]

        public string chosenAvatar;
        public string avatarModel;
        public AvatarGender avatarGender;
        public AvatarSkinTone avatarSkinTone;

        public enum AvatarGender { Male, Female, Neutral };
        public enum AvatarSkinTone { Black, White, Asian };

        [Header("Choose clothing activity")]

        [Tooltip("Results of clothing selection")]
        public Dictionary<string, bool> resultsCache;
        public string clothingSelectorReport;

        [Header("Current activity")]

        [Tooltip("Handy for caching results when there may be a new state which refers to previous state's data")]
        public int currentActivityScore;
        public int currentActivityBonusPoints;

        [Header("CurrentExperience tally")]

        public int currentExperienceMaxValue = 0; // Gets built up with activities
        public int currentExperienceTotalScore = 0; // ditto
        public int currentExperienceBonusPoints = 0;
        public float passUnitValue = .85f;

        public void ResetCurrentExperienceScores()
        {
            currentExperienceMaxValue = 0;
            currentExperienceTotalScore = 0;
            currentExperienceBonusPoints = 0;
        }

        public bool DecentScoreForCurrentExperience()
        {
            return (currentExperienceTotalScore >= (currentExperienceMaxValue * passUnitValue));
        }

        public void PlayReward()
        {
            LeanAudio.play(rewardSound);
        }

        public void SetAvatarInfo(string chosen)
        {
            avatarModel = chosen;

            if (chosen.StartsWith("Boy"))
            {
                avatarGender = AvatarGender.Male;

                if (chosen.EndsWith("A"))
                    avatarSkinTone = AvatarSkinTone.Asian;
                if (chosen.EndsWith("B"))
                    avatarSkinTone = AvatarSkinTone.Black;
                else
                    avatarSkinTone = AvatarSkinTone.White;
            }
            else if (chosen.StartsWith("Girl"))
            {
                avatarGender = AvatarGender.Female;

                if (chosen.EndsWith("A"))
                    avatarSkinTone = AvatarSkinTone.Asian;
                else if (chosen.EndsWith("B"))
                    avatarSkinTone = AvatarSkinTone.Black;
                else
                    avatarSkinTone = AvatarSkinTone.White;

            }
            else
            {
                avatarGender = AvatarGender.Neutral;
                avatarSkinTone = AvatarSkinTone.Asian; // sort of middle
            }
        }

        public void OverrideReticleInset(float temp)
        {
            inset = temp;
        }

        public void ResetReticleInset()
        {
            inset = defaultReticleInset;
        }

        public void ResetRaycastDistance()
        {
            raycastDistance = defaultRaycastDistance;
        }

        public void PushRaycastDistance(float newVal)
        {
            raycastDistance = newVal;
        }

        /// <summary>
        /// Not typical push and pop as they 
        /// must be idempotent to prevent going
        /// out of whack if not balanced from
        /// one state to another
        /// </summary>
        public void PopRaycastDistance()
        {
            ResetRaycastDistance();
        }

        public void PushBeamExtensionSpeed(float newVal)
        {
            cacheBeamExtensionSpeed = beamExtensionSpeed;
            beamExtensionSpeed = newVal;
        }

        public void PopBeamExtensionSpeed()
        {
            if (cacheBeamExtensionSpeed > 0f)
                beamExtensionSpeed = cacheBeamExtensionSpeed;
        }

        public void PushAmbientLight(Color light)
        {
            cacheAmbientLight = RenderSettings.ambientLight;
            RenderSettings.ambientLight = light;
        }

        public void PopAmbientLight()
        {
            RenderSettings.ambientLight = cacheAmbientLight;
        }

        public float TextDisplayTime(string text)
        {
            float units = (float)text.Length / infoLengthUnit;
            if (units < 1)
                units = 1;
            return infoDisplayTime * units;
        }

        // Temp storage for non-persistent runtime values,
        // such as for saving player position
        public Vector3 cachedPosition;
        public Quaternion cachedRotation;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        /// Object management
        ///////////////////////////////////////////////////////////////////////////////////////////////

        private static ActivitySettings _asset;

        [Header("Initialisers")]

        [Tooltip("Can be modified in inspector but not via code")]
        [SerializeField]
        private readonly float defaultRaycastDistance = 15f;
        [SerializeField]
        private readonly float defaultBeamExtensionSpeed = 100;
        [SerializeField]
        private float cacheRaycastDistance;
        [SerializeField]
        private float cacheBeamExtensionSpeed;
        [SerializeField]
        private Color cacheAmbientLight;
        [SerializeField]
        private readonly float defaultReticleInset = .005f;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            raycastDistance = defaultRaycastDistance;
            inset = defaultReticleInset;
            beamExtensionSpeed = defaultBeamExtensionSpeed;
            showReticleAlways = false;
        }

        public static ActivitySettings Asset
        {
            get
            {
                if (!_asset)
                    _asset = Resources.Load<ActivitySettings>("ActivitySettings");
                return _asset;
            }
        }

        public bool Testing
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return false;
#else
                return testing;
#endif
            }
        }
    }
}
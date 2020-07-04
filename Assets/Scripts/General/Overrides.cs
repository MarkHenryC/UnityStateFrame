using System;
using System.IO;
using UnityEngine;

namespace QS
{
    [Serializable]
    public class Overrides
    {
#if false // Can only be used with a more advanced json parser, not Unity's
        public bool? masterNavOverride; // Overrides the runtime check
        public int? startActivityIndex; // only useful with override & overrides one on ActivityManager if non-null        
        public float? playerSpeed;
        public float? damageCapacity;
        public bool? allowSkipScenes;
        public string stringData;
        public string somethingElse;

        public static void LoadOverrides()
        {
            var overrides = GetOverrides();
            if (overrides.masterNavOverride.HasValue)
            {
                ActivitySettings.Asset.masterNavOverride = overrides.masterNavOverride.Value;
                if (overrides.startActivityIndex.HasValue)
                    ActivitySettings.Asset.startActivityIndex = overrides.startActivityIndex.Value;
            }

            if (overrides.allowSkipScenes.HasValue)
                ActivitySettings.Asset.AllowSkipScenes = overrides.allowSkipScenes.Value;
            if (overrides.playerSpeed.HasValue)
                ActivitySettings.Asset.playerSpeed = overrides.playerSpeed.Value;
            if (overrides.damageCapacity.HasValue)
                ActivitySettings.Asset.damageCapacity = overrides.challengeDamageCapacity.Value;
        }
#else
        public bool masterNavOverride; // Overrides the runtime check
        public bool allowSkipScenes; // Allows skip panel to be shown
        public int startActivityIndex; // only useful with masterNavOverride
        public float playerSpeed; // For testing or tweaking speeds in hazard challenge
        public float damageCapacity; // For testing or tweaking durability in hazard challenge
        public bool debugFlag; // For testing specific high-volume debug output
        public bool showTouchpadY; // For testing dodgy touchpad readings

        public static void LoadOverrides()
        {
            Overrides overrides = GetOverrides();
            // OK to set to defaults (false, 0)
            ActivitySettings.Asset.masterNavOverride = overrides.masterNavOverride;
            ActivitySettings.Asset.startActivityIndex = overrides.startActivityIndex;
            ActivitySettings.Asset.AllowSkipScenes = overrides.allowSkipScenes;
            ActivitySettings.Asset.masterDebugFlag = overrides.debugFlag;
            ActivitySettings.Asset.showTouchpadY = overrides.showTouchpadY;

            // Don't set these to defaults if not listed (i.e. 0.0)
            // Unity JsonUtility doesn't support nullables, so check for default value
            // Note this only works if zero is never a useful value, but OK if it's 
            // an acceptable default, such as with startActivityIndex
            if (overrides.playerSpeed > 0f)
                ActivitySettings.Asset.playerSpeed = overrides.playerSpeed;
            if (overrides.damageCapacity > 0f)
                ActivitySettings.Asset.damageCapacity = overrides.damageCapacity;
        }

#endif
        private static Overrides GetOverrides()
        {
            string jsonPath = Utils.VIDEO_PATH + "Overrides.json";
            if (!File.Exists(jsonPath))
            {
                Debug.Log("No json override file available: " + jsonPath);
                return new Overrides();
            }

            Debug.Log("Loading override from: " + jsonPath);

            string data = File.ReadAllText(jsonPath);

            return JsonUtility.FromJson<Overrides>(data);
        }

    }
}
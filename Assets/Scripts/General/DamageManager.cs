using System;
using UnityEngine;

namespace QS
{
    public class DamageManager : MonoBehaviour
    {
        public Hazard[] hazards;
        public Action<Hazard, float> onDamage;
        public Action<Hazard> onBump;
        public bool allSelfOscillating = true;

        public void Clear()
        {
            foreach (Hazard haz in hazards)
                haz.Clear();
        }

        private void Start()
        {
            foreach (Hazard haz in hazards)
            {
                if (allSelfOscillating)
                    haz.selfOscillating = true;
                haz.callOnDamage = AddDamage;
                if (haz.bumpDamage)
                    haz.callOnBump = Bumped;
            }
        }

        private void AddDamage(Hazard haz, float currentDamage)
        {
            Debug.Log("DamageManager: AddDamage");
            if (onDamage != null)
                onDamage(haz, currentDamage);
        }

        private void Bumped(Hazard haz)
        {
            Debug.Log("DamageManager: Bumped");
            if (onBump != null)
                onBump(haz);
        }
    }
}
using UnityEngine;

namespace QS
{
    public class DialHandler : UiPanel
    {
        public Transform rotateable;

        protected string metric;
        protected float minRange, maxRange;
        protected float currentCoeff;
        protected float range;
        protected int places;
        protected string formatString;

        public virtual float SetMetric(float min, float max, string name, int decimalPlaces = 2, float initValue = 0f)
        {
            minRange = min;
            maxRange = max;
            range = maxRange - minRange;
            metric = name;
            places = decimalPlaces;
            formatString = string.Format("F{0:D}", places);

            return Set(initValue);
        }

        public virtual float Set(float coeff)
        {
            currentCoeff = coeff;
            SetDial(currentCoeff);

            float amount = minRange + range * coeff;
            if (places == 0)
            {
                if (amount < 1f)
                    amount = 0f;
                else
                    amount = Mathf.Min(Mathf.RoundToInt(amount + 0.5f), maxRange);
            }
            infoText.text = amount.ToString(formatString) + " " + metric;

            return amount;
        }

        /// <summary>
        /// Set based on current coeff
        /// </summary>
        protected virtual void SetDial(float f)
        {
            float angle = f * 180f - 90f;
            rotateable.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
using UnityEngine;

namespace QS
{
    /// <summary>
    /// A class that accepts a float UnitValue
    /// </summary>
    public abstract class UnitFloatReceiver : MonoBehaviour
    {
        public ActionFloatProvider newValueReceiver;

        protected void OnEnable()
        {
            if (newValueReceiver)
                newValueReceiver.AddResponder(SetValue);
        }

        protected void OnDisable()
        {
            if (newValueReceiver)
                newValueReceiver.RemoveResponder(SetValue);
        }

        /// <summary>
        /// unitVal range 0.0 .. 1.0
        /// </summary>
        /// <param name="unitVal"></param>
        public abstract void SetValue(IUnitValueProvider provider, float unitVal);
    }

}
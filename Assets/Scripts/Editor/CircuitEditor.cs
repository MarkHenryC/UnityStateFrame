using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(Circuit), true)]
    public class CircuitEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Circuit script = (Circuit)target;

            if (GUILayout.Button("Trace"))
            {
                script.Trace(Circuit.TraceTrigger.Test);
            }
        }

    }
}
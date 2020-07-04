using UnityEngine;
using UnityEditor;

namespace QS
{    
    [CustomEditor(typeof(Switch), true)]
    public class SwitchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Switch script = (Switch)target;

            if (GUILayout.Button("Up"))
            {
                script.SetSwitch(true);
            }
            else if (GUILayout.Button("Down"))
            {
                script.SetSwitch(false);
            }

        }

    }
}
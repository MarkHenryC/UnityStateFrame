using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(ActivityBase), true)]
    [CanEditMultipleObjects]
    public class ActivityBaseEditor : Editor
    {
        private static GUIStyle divider = null;

        public override void OnInspectorGUI()
        {
            if (divider == null)
                divider = new GUIStyle(GUI.skin.box);

            DrawDefaultInspector();

            ActivityBase aScript = (ActivityBase)target;

            GUILayout.BeginHorizontal(divider);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Hide visuals"))
            {
                aScript.SetVisuals(false);
            }
            else if (GUILayout.Button("Show visuals"))
            {
                aScript.SetVisuals(true);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

        }
    }
}
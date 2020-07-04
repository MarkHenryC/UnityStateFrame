using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(StateProcessor), true)]
	public class StateProcessorEditor : Editor 
	{
        private static GUIStyle divider = null;

        public override void OnInspectorGUI()
        {
            if (divider == null)
                divider = new GUIStyle(GUI.skin.box);

            DrawDefaultInspector();

            StateProcessor script = (StateProcessor)target;

            if (GUILayout.Button("Align camera"))
            {
                ControllerInput.Instance.SetPlayerAspect(script.playerInitialAspect);
            }

            GUILayout.BeginHorizontal(divider);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Hide visuals"))
            {
                script.ShowVisuals(false);
            }
            else if (GUILayout.Button("Show visuals"))
            {
                script.ShowVisuals(true);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

        }

    }
}
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace QS
{
    [CustomEditor(typeof(ActivityManager), true)]
    [CanEditMultipleObjects]
    public class ActivityManagerEditor : Editor
    {
        private static GUIStyle divider = null;
 
        public override void OnInspectorGUI()
        {
            if (divider == null)
                divider = new GUIStyle(GUI.skin.box);

            DrawDefaultInspector();

            ActivityManager aScript = (ActivityManager)target;

            GUILayout.BeginVertical(divider);
            EditorGUILayout.BeginVertical();

            GUILayout.Label("\nNavigation override is always\nswitched off in builds.\n");
            EditorGUILayout.BeginHorizontal();
            bool enableOverride = GUILayout.Toggle(ActivitySettings.Asset.navigationOverride, "Navigation override");
            if (enableOverride != ActivitySettings.Asset.navigationOverride)
                ActivitySettings.Asset.navigationOverride = enableOverride;
            GUILayout.Label("Start activity: ");
            EditorGUILayout.EndHorizontal();
            aScript.startActivityName = GUILayout.TextField(aScript.startActivityName);
            if (GUILayout.Button("Hide all visuals"))
            {
                Undo.RecordObject(aScript, "Hide all visuals");
                
                aScript.ClearVisuals();

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace QS
{
    [CustomEditor(typeof(Graspable), true)]
    [CanEditMultipleObjects]
    public class GraspableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Graspable gScript = (Graspable)target;

            if (GUILayout.Button("Transform to home"))
            {
                Undo.RecordObject(gScript, "Transform to home");
                gScript.TransformToHome();
            }
            else if (GUILayout.Button("Transform to dest"))
            {
                Undo.RecordObject(gScript, "Transform to dest");
                gScript.TransformToDest();
            }
            else if (GUILayout.Button("!! Set home transform !!"))
            {
                Undo.RecordObject(gScript, "Set home transform");
                gScript.SetHomeTransform();
            }
            else if (GUILayout.Button("!! Set dest transform !!"))
            {
                Undo.RecordObject(gScript, "Set destination transform");
                gScript.SetDestTransform();
            }
        }
    }
}
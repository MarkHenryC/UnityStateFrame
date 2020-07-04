using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(AlignTransform), true)]
    public class AlignTransformEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AlignTransform script = (AlignTransform)target;

            if (GUILayout.Button("Align"))
            {
                script.Align();
            }

            if (GUILayout.Button("Update from saved runtime data"))
            {
                script.LoadSavedPosition();
            }

        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(InfoPanel), true)]
    [CanEditMultipleObjects]
    public class InfoPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            InfoPanel infoPanelScript = (InfoPanel)target;

            if (GUILayout.Button("Set home position"))
            {
                infoPanelScript.Flip(false);
            }
            else if (GUILayout.Button("Set flipped position"))
            {
                infoPanelScript.Flip(true);
            }
        }
    }
}
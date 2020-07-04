using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(SelectionMenu), true)]
    [CanEditMultipleObjects]
    public class SelectionMenuEditor : Editor
    {        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SelectionMenu sScript = (SelectionMenu)target;

            if (GUILayout.Button("Move to home position of Selection groups"))
            {
                sScript.MoveToHomePositions();
            }
            else if (GUILayout.Button("Move to destination position of Selection groups"))
            {
                sScript.MoveToDestPositions();
            }
            else if (GUILayout.Button("!! Set home position of Selection groups !!"))
            {
                sScript.SetHomePositions();
            }
            else if (GUILayout.Button("!! Set destination position of Selection groups !!"))
            {
                sScript.SetDestPositions();
            }
        }
    }
}
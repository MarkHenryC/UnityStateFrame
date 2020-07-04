using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System;

namespace QS
{
    [CustomEditor(typeof(Morph), true)]
	public class MorphEditor : Editor
	{
        private string expressionName;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Morph script = (Morph)target;
            expressionName = GUILayout.TextField(expressionName);

            if (GUILayout.Button("Setup"))
            {
                script.CountBlendShapes();
            }

            else if (GUILayout.Button("Create expression"))
            {
                Undo.RecordObject(script, "Create expression " + expressionName);
                script.CreateExpression(expressionName);
            }
            else if (GUILayout.Button("Set expression"))
            {
                Undo.RecordObject(script, "Set to expression " + expressionName);
                script.SetExpressionImmediate(expressionName);
            }

            else if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(script, "Reset expression " + expressionName);
                script.ResetShapes();
            }
        }
        private void OnEnable()
        {
            Morph script = (Morph)target;
            script.CountBlendShapes();
        }
    }
}
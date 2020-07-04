using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System;

namespace QS
{
    [CustomEditor(typeof(FacialExpressions), true)]
	public class FacialExpressionEditor : Editor
	{
        private string expressionName;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FacialExpressions fScript = (FacialExpressions)target;
            expressionName = GUILayout.TextField(expressionName);

            if (GUILayout.Button("Setup"))
            {
                fScript.CountBlendShapes();
            }

            else if (GUILayout.Button("Create expression"))
            {
                Undo.RecordObject(fScript, "Create expression " + expressionName);
                fScript.CreateExpression(expressionName);
            }
            else if (GUILayout.Button("Set to named expression"))
            {
                Undo.RecordObject(fScript, "Set to expression " + expressionName);
                fScript.SetExpressionImmediate(expressionName);
            }

            else if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(fScript, "Reset expression " + expressionName);
                fScript.ResetShapes();
            }
        }
        private void OnEnable()
        {
            FacialExpressions fScript = (FacialExpressions)target;
            fScript.CountBlendShapes();
        }
    }
}
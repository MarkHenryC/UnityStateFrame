using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(BlendShapeManager), true)]
    public class BlendShapeManagerEditor : Editor
    {
        private string expressionName;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BlendShapeManager bScript = (BlendShapeManager)target;

            if (GUILayout.Button("Setup"))
            {
                bScript.CountBlendShapes();
            }

            expressionName = GUILayout.TextField(expressionName);

            if (GUILayout.Button("Create expression"))
            {
                bScript.CreateExpression(expressionName);
            }
            else if (GUILayout.Button("Set expression"))
            {
                bScript.SetExpressionImmediate(expressionName);
            }
            else if (GUILayout.Button("Reset"))
            {
                bScript.ResetShapes();
            }
        }

        private void OnEnable()
        {
            BlendShapeManager bScript = (BlendShapeManager)target;
            bScript.CountBlendShapes();
        }
    }
}
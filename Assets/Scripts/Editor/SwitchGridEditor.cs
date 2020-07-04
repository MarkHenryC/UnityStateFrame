using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(SwitchGrid), true)]
	public class SwitchGridEditorEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SwitchGrid script = (SwitchGrid)target;

            if (GUILayout.Button("Create grid"))
                script.CreateGrid();
            else if (GUILayout.Button("Clear grid"))
                script.ClearGrid();
        }

	}
}
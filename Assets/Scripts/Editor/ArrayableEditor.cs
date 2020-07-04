using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(Arrayable), true)]
	public class ArrayableEditorEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Arrayable script = (Arrayable)target;

            if (GUILayout.Button("Set start position"))
            {
                Undo.RecordObject(script, "Set start position");
                script.SetStartPosition();
            }
            else if (GUILayout.Button("Generate"))
            {
                Undo.RecordObject(script, "Generate");
                script.Generate();
            }
            else if (GUILayout.Button("Clear"))
            {
                Undo.RecordObject(script, "Clear");
                script.Clear(true);
            }
        }

	}
}
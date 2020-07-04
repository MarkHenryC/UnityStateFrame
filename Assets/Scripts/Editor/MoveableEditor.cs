using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(Moveable), true)]
	public class MoveableEditorEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Moveable script = (Moveable)target;

            if (GUILayout.Button("Set start position"))
            {
                Undo.RecordObject(script, "Set start position");
                script.SetStartPosition();
            }
            else if (GUILayout.Button("Set end position"))
            {
                Undo.RecordObject(script, "Set end position");
                script.SetEndPosition();
            }
            else if (GUILayout.Button("Move to start position"))
            {
                Undo.RecordObject(script, "Move to start position");
                script.SetPosition(0f);
            }
            else if (GUILayout.Button("Move to end position"))
            {
                Undo.RecordObject(script, "Move to end position");
                script.SetPosition(1f);
            }

        }

    }
}
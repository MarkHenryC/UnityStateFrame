using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(DialogManager), true)]
	public class DialogManagerEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DialogManager script = (DialogManager)target;

            if (GUILayout.Button("Parse"))
            {
                script.Parse();
            }
        }

	}
}
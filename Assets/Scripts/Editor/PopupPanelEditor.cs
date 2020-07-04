using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(PopupPanel), true)]
	public class PopupPanelEditorEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PopupPanel script = (PopupPanel)target;

            if (GUILayout.Button("Popup"))
                script.PopUp(3f);
            else if (GUILayout.Button("Return"))
                script.PopDown();
        }

	}
}
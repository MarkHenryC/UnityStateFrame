using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(DialogOptions), true)]
	public class NewEditorScriptEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DialogOptions script = (DialogOptions)target;

            if (GUILayout.Button("Connections"))
            {
                DialogOptions[] obs = new DialogOptions[script.options.Length];
                
                for (int i = 0; i < script.options.Length; i++)
                {
                    obs[i] = script.options[i].destination;
                }
                Selection.objects = obs;
            }
        }

	}
}
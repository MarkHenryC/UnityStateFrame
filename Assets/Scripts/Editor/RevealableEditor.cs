using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(Revealable), true)]
	public class RevealableEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Revealable script = (Revealable)target;

            if (GUILayout.Button("Activate"))
            {
                script.Reveal();
            }
            else if (GUILayout.Button("Deactivate"))
            {
                script.SetToDefault();
            }

        }

    }
}
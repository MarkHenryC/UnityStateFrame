using UnityEngine;
using UnityEditor;

namespace QS
{
	[CustomEditor(typeof(ConversationParser), true)]
	public class ConversationParserEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ConversationParser script = (ConversationParser)target;

            if (GUILayout.Button("Print conversation paths"))
                script.Parse();
            else if (GUILayout.Button("Print path codes"))
                script.ParseBrief();

        }
    }
}
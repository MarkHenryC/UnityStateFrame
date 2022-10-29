#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace QS
{
    // We have to remove the Editor part of the name,
    // because in Unity script templates it uses only
    // the file name to define the class. In an editor
    // script we need to refer to the target class, but
    // can't use that name here as it'll cause a class
    // with the target class name.
    [CustomEditor(typeof(PolyCover), true)]
    public class PolyCoverEditorEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PolyCover script = (PolyCover)target;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Runtime dissolve"))
                {
                    // Call method here
                    script.Dissolve();
                }
                if (GUILayout.Button("Runtime batch dissolve"))
                {
                    // Call method here
                    script.BatchDissolve();
                }

            }

        }

    }
}
#endif
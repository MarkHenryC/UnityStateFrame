using UnityEngine;
using UnityEditor;

// We have to remove the Editor part of the name,
// because in Unity script templates it uses only
// the file name to define the class. In an editor
// script we need to refer to the target class, but
// can't use that name here as it'll cause a class
// with the target class name.
namespace QS
{
	[CustomEditor(typeof(GrabbableManager), true)]
    [CanEditMultipleObjects]
	public class GrabbableManagerEditorEditor : Editor 
	{
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GrabbableManager script = (GrabbableManager)target;

            if (GUILayout.Button("Populate from subfolder"))
            {
                Undo.RecordObjects(script.grabbables, "Load from subfolder");

                script.LoadFromSubfolder();
            }
            else if (GUILayout.Button("Move to home"))
            {
                Undo.RecordObjects(script.grabbables, "Move all to home");

                script.MoveAllToHome();
            }
            else if (GUILayout.Button("Move to dest"))
            {
                Undo.RecordObjects(script.grabbables, "Move all to dest");

                script.MoveAllToDest();
            }
            else if (!script.lockHomePositions && GUILayout.Button("Set current as home"))
            {
                Undo.RecordObjects(script.grabbables, "Set current as home");

                script.SetAllAsHome();
            }
            else if (!script.lockDestinationPositions && GUILayout.Button("Set current as dest"))
            {
                Undo.RecordObjects(script.grabbables, "Set current as dest");

                script.SetAllAsDest();
            }
        }

    }
}
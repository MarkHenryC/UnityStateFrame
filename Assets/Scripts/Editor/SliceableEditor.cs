using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(Sliceable), true)]
    public class SliceableEditor : GraspableEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Sliceable sScript = (Sliceable)target;

            if (GUILayout.Button("Set begin slice range"))
            {
                sScript.SetStartSlicePos();
            }
            else if (GUILayout.Button("Set end slice range"))
            {
                sScript.SetEndSlicePos();
            }
            else if (GUILayout.Button("Create slices"))
            {
                sScript.EditorMarkSlices();
            }
            if (sScript.chop)
            {
                if (GUILayout.Button("Set begin chop range"))
                {
                    sScript.SetStartChopPos();
                }
                else if (GUILayout.Button("Set end chop range"))
                {
                    sScript.SetEndChopPos();
                }
            }
        }
    }
}
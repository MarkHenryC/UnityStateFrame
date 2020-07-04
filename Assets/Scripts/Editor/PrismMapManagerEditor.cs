using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace QS
{
	public class PrismMapManagerEditor : Editor
	{
        [CustomEditor(typeof(PrismMapManager), true)]
        public class HairMeshCreatorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                PrismMapManager script = (PrismMapManager)target;

                if (GUILayout.Button("Set mapping"))
                {
                    script.SetTextureMapping();
                }
                else if (GUILayout.Button("Clear"))
                {
                    script.ClearAll();
                }

            }
        }
    }
}
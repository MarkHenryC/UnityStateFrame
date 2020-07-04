using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QS
{
    [CustomEditor(typeof(AudioLooper), true)]
    public class EnergyHudEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AudioLooper alScript = (AudioLooper)target;

            if (GUILayout.Button("Play"))
            {
                alScript.Play();
            }
            else if (GUILayout.Button("Playthrough"))
            {
                alScript.PlayThrough();
            }
            else if (GUILayout.Button("Fadeout"))
            {
                alScript.Fadeout();
            }

        }
    }
}
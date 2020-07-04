using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class DynamicMaterial : MonoBehaviour 
	{
        [Tooltip("Optional. If not set, gets base material from component renderer")]
        public Material template;
        [Tooltip("Set colour on Awake?")]
        public bool setOnStart;
        [Tooltip("Specify initial colour.")]
        public Color32 startColour;
        [Tooltip("Handy shortcut for using this as a highlighter.")]
        public Color32 highlightColour;

        private Material clone;
        private Color32 originalColour;

		void Awake () 
		{
            var r = GetComponent<Renderer>();
            if (r)
            {

                if (template)
                    clone = new Material(template);
                else
                    clone = new Material(r.sharedMaterial);

                originalColour = clone.GetColor("_Color");

                r.sharedMaterial = clone;
                if (setOnStart)
                    SetColour(startColour);
            }
            else
                Debug.LogError("No renderer for DynamicMaterial component on " + gameObject.name);
		}
		
        public void SetColour(Color32 col)
        {
            if (clone)
                clone.SetColor("_Color", col);
        }

        public Color GetColor()
        {
            if (clone)
                return clone.GetColor("_Color");
            else
                return Color.clear;
        }

        public void Highlight(bool highlight)
        {
            if (highlight)
                SetColour(highlightColour);
            else
                SetColour(originalColour);
        }

        private void OnDestroy()
        {
            if (clone)
                Destroy(clone);
        }
    }
}
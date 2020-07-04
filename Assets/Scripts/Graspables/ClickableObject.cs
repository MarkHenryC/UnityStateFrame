using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class ClickableObject : ButtonPanel
	{
        public GameObject selectedMarker;
        public Material sourceMaterial;
        public Color idle, highlighted, clicked;
        public float alpha;

        public bool Selected { get; private set; }

        public Action<ClickableObject> OnClick;
        public Action<ClickableObject> PointerEnter;
        public Action<ClickableObject> PointerExit;

        private Material ourMaterial; // Keep custom material
        private Color32 originalColor;

        protected override void Awake()
        {
            base.Awake();

            if (sourceMaterial)
                ourMaterial = new Material(sourceMaterial);
            else
                ourMaterial = new Material(GetComponent<Renderer>().sharedMaterial);

            originalColor = ourMaterial.color;

            GetComponent<Renderer>().sharedMaterial = ourMaterial;

            if (alpha != 0)
            {
                alpha = Mathf.Clamp(alpha, 0, 1f);
                idle.a = alpha;
                highlighted.a = alpha;
                clicked.a = alpha;
            }

            SetIdle();
        }

        private void OnDestroy()
        {
            Destroy(ourMaterial);
        }

        public void Select(bool select)
        {
            if (selectedMarker)
                selectedMarker.SetActive(select);
            Selected = select;
            if (Selected)
                ourMaterial.color = clicked;
            else
                SetIdle();
        }

        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            ourMaterial.color = highlighted;
            PointerEnter?.Invoke(this);
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            if (!Selected)
                SetIdle();
            PointerExit?.Invoke(this);
        }

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);
            ourMaterial.color = clicked;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);

            Select(true);

            if (OnClick != null)
                OnClick(this);            
        }

        public override void Clear()
        {
            base.Clear();

            Selected = false;

            if (selectedMarker)
                selectedMarker.SetActive(false);
        }

        private void SetIdle()
        {
            if (idle.a > 0)
                ourMaterial.color = idle;
            else
                ourMaterial.color = originalColor;
        }
    }
}
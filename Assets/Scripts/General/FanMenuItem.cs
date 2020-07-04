using UnityEngine;
using TMPro;
using UnityEngine.Events;

namespace QS
{
    public class FanMenuItem : ButtonPanel
    {
        public SpriteRenderer spriteRenderer;
        public TextMeshPro tmpText;
        public UnityEvent ueResponse;
        public System.Action<int> response;

        private int ourIndex;

        public void Create(string text, Sprite image, UnityEvent response)
        {
            spriteRenderer.sprite = image;
            tmpText.text = text;
            ueResponse = response;
        }

        public void Create(string text, int index, System.Action<int> onSelect)
        {
            tmpText.text = text;
            response = onSelect;
            ourIndex = index;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);
            if (pointerWithin)
            {
                ueResponse?.Invoke();
                response?.Invoke(ourIndex);
            }
        }

        protected override void HandleEnter()
        {
            LeanTween.moveLocalZ(gameObject, -zMove, moveTime);
        }

        protected override void HandleExit()
        {
            LeanTween.moveLocalZ(gameObject, 0f, moveTime);
        }
    }
}
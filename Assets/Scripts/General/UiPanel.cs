using System;
using TMPro;
using UnityEngine;

namespace QS
{
    public class UiPanel : MonoBehaviour
    {
        public TextMeshPro infoText;
        public bool followPlayer;

        protected float currentDelay;

        public virtual UiPanel Place()
        {
            Utils.PlaceInfoPanelBeforeCamera(transform, ActivitySettings.Asset.infoPanelFromCameraDistance, ControllerInput.Instance.Player);
            return this;
        }

        public virtual UiPanel PlaceClose()
        {
            Utils.PlaceInfoPanelCloseBeforeCamera(transform, ActivitySettings.Asset.closeupPanelFromCameraDistance, ControllerInput.Instance.Player);
            return this;
        }

        public virtual UiPanel SetText(string text, bool show = true)
        {
            if (text == null) // shortcut to clear and hide, called by ITextReceiver client
            {
                infoText.text = "";
                gameObject.SetActive(false);
            }
            else
            {
                infoText.text = text;
                gameObject.SetActive(show);
            }
            return this;
        }

        public virtual UiPanel Show(bool show = true)
        {
            currentDelay = 0f;
            gameObject.SetActive(show);
            return this;
        }

        public virtual UiPanel ShowAfter(float delay)
        {
            LeanTween.delayedCall(delay, () => { gameObject.SetActive(true); currentDelay = 0f; });
            currentDelay = delay;

            return this;
        }

        public virtual UiPanel ShowFor(float seconds, Action then = null, bool close = true)
        {
            gameObject.SetActive(true);
            LeanTween.delayedCall(seconds + currentDelay, () =>
            {
                gameObject.SetActive(!close);
                if (then != null)
                    then();
            });
            return this;
        }

        public virtual UiPanel ShowFor(string text, Action then = null, bool close = true)
        {
            SetText(text);
            float seconds = ActivitySettings.Asset.TextDisplayTime(text);
            gameObject.SetActive(true);
            LeanTween.delayedCall(seconds + currentDelay, () =>
            {
                gameObject.SetActive(!close);
                then?.Invoke();
            });

            return this;
        }

        protected virtual void Update()
        {
            if (followPlayer)
                Utils.FaceCamera(transform, ControllerInput.Instance.Player);
        }
    }
}

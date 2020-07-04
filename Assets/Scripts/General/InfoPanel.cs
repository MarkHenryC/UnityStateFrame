using UnityEngine;

namespace QS
{
    public class InfoPanel : UiPanel
    {
        public GameObject backing; // added for speech bubbles so we can reverse
        public System.Action OnExpired; // call when timeout
        public bool flipped;
        public TextBox bonusText;

        private Quaternion normalRotation;

        public void Awake()
        {
            if (backing)
                normalRotation = backing.transform.localRotation;
            if (flipped)
                Flip(true);
            ShowBonus(false);
        }

        public override UiPanel Show(bool show = true)
        {
            ShowBonus(false);

            return base.Show(show);
        }

        public void Flip(bool flip)
        {
            if (backing)
            {
                backing.transform.rotation = normalRotation;
                if (flip)
                    backing.transform.Rotate(0f, 180f, 0f, Space.Self);
            }
        }

        public void ShowBonus(string text)
        {
            if (bonusText)
                bonusText.SetText(text);
        }

        public void TryBonus(string text = "BONUS: ")
        {
            if (ActivitySettings.Asset.currentActivityBonusPoints > 0)
            {
                if (bonusText)
                    bonusText.SetText(text + ActivitySettings.Asset.currentActivityBonusPoints);
            }
        }

        public void TryBonus(int points, string text = "BONUS: ")
        {
            if (points > 0)
            {
                if (bonusText)
                    bonusText.SetText(text + points);
            }
        }

        public void ShowBonus(bool show)
        {
            if (bonusText)
                bonusText.Show(show);
        }
    }
}
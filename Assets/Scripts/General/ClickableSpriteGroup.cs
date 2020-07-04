using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Linq;

namespace QS
{
    /// <summary>
    /// Each instance of this class is a menu
    /// selection. If only a single item, it's
    /// treated as a toggle.
    /// </summary>
    public class ClickableSpriteGroup : MonoBehaviour
    {
        public ClickableSprite[] clickableSprites;
        [Tooltip("Index of the correct selection. For a single toggled item, 0 is correct and -1 incorrect if activated")]
        public int correctIndex;

        public bool SelectedCorrect { get; private set; }
        public int SelectedIndex { get; private set; }

        private bool anySelected;
        private bool toggled;
        private bool activated;

        void Awake()
        {
            if (clickableSprites.Length == 1)
                toggled = true;

            for (int i = 0; i < clickableSprites.Length; i++)
            {
                var sel = clickableSprites[i];
                sel.AllocatedIndex = i;
                sel.OnClick = Selected;
                sel.TransformToHome();
            }            
        }

        public bool Interactable
        {
            set
            {
                foreach (var s in clickableSprites)
                    s.Dormant = !value;
            }
        }

        public bool AnySelected
        {
            get
            {
                if (toggled)
                    return true; // Don't need to select anything with toggle
                else 
                    return anySelected;
            }
        }

        public void ResetAll()
        {
            for (int i = 0; i < clickableSprites.Length; i++)
            {
                var sel = clickableSprites[i];
                sel.Clear();
            }
            anySelected = false;

            MoveToHomePositions();
        }

        public ClickableSprite GetSelectedSprite()
        {
            if (toggled)
            {
                return clickableSprites[0];
            }
            else
            {
                if (anySelected)
                    return clickableSprites[SelectedIndex];
                else
                    return null;
            }

        }

        private void Selected(ClickableSprite cs)
        {
            if (toggled)
            {
                activated = !activated;

                if (correctIndex == -1) // Means selecting single item is incorrect
                    SelectedCorrect = !activated;
                else
                    SelectedCorrect = activated;

                if (activated)
                    cs.TransformToDest();
                else
                    cs.TransformToHome();
            }
            else
            {
                if (anySelected)
                    clickableSprites[SelectedIndex].TransformToHome();
                else
                    anySelected = true;

                SelectedIndex = cs.AllocatedIndex;
                clickableSprites[SelectedIndex].TransformToDest();

                if (cs.AllocatedIndex == correctIndex)
                    SelectedCorrect = true;
                else
                    SelectedCorrect = false;
            }
        }

        /// <summary>
        /// For setting in edit mode
        /// </summary>
        public void SetHomePositions(bool local = false)
        {
            foreach (var s in clickableSprites)
                s.SetHomeTransform(local);
        }

        public void SetDestPositions(bool local = false)
        {
            foreach (var s in clickableSprites)
                s.SetDestTransform(local);
        }

        public void MoveToHomePositions(bool local = false)
        {
            foreach (var s in clickableSprites)
                s.TransformToHome(local);
        }

        public void MoveToDestPositions(bool local = false)
        {
            foreach (var s in clickableSprites)
                s.TransformToDest(local);
        }

    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using System.Linq;

namespace QS
{
    /// <summary>
    /// Contains a number of ClickableSpriteGroups
    /// </summary>
    public class SelectionMenu : MonoBehaviour
    {
        public ClickableSpriteGroup[] selectableGroups;
        public bool lookAtPlayer, test;
        public bool localTransforms;

        private void Start()
        {
            if (test)
                Show();
        }

        void Update()
        {
            if (lookAtPlayer)
                Utils.FaceCamera(transform);
        }

        public void ResetAll()
        {
            foreach (var g in selectableGroups)
                g.ResetAll();
        }

        public bool Interactable
        {
            set
            {
                foreach (var g in selectableGroups)
                    g.Interactable = value;
            }
        }

        public Dictionary<string, bool> GetResults()
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();

            foreach (var group in selectableGroups)
            {
                ClickableSprite cs = group.GetSelectedSprite();
                results[cs.name] = group.SelectedCorrect;
            }
            return results;
        }

        public bool AnySelected
        {
            get
            {
                return selectableGroups.Any(s => s.AnySelected);
            }
        }

        /// <summary>
        /// Toggle will always count as a selection
        /// </summary>
        public bool AllSelected
        {
            get
            {
                return selectableGroups.All(s => s.AnySelected);
            }
        }

        public void Show(bool show = true)
        {
            gameObject.SetActive(show);
        }

        public void SetHomePositions()
        {
            foreach (var g in selectableGroups)
                g.SetHomePositions(localTransforms);
        }

        public void SetDestPositions()
        {
            foreach (var g in selectableGroups)
                g.SetDestPositions(localTransforms);
        }

        public void MoveToHomePositions()
        {
            foreach (var g in selectableGroups)
                g.MoveToHomePositions(localTransforms);
        }

        public void MoveToDestPositions()
        {
            foreach (var g in selectableGroups)
                g.MoveToDestPositions(localTransforms);
        }
    }
}
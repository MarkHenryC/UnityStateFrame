using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace QS
{
    public class Tooltip : MonoBehaviour
    {
        private TextMeshPro tmpText;

        void Awake()
        {
            tmpText = GetComponentInChildren<TextMeshPro>();
            Activate(false);
        }

        void Update()
        {

        }

        public void SetText(string text)
        {
            if (tmpText)
                tmpText.text = text;
        }

        public void Activate(bool active)
        {
            if (tmpText)
                tmpText.enabled = active;
        }
    }

}
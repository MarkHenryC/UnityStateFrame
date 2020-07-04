using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace QS
{
	public class TextBox : MonoBehaviour 
	{
        public TextMeshPro data;

        public void SetText(string text, bool show = true)
        {
            data.text = text;
            gameObject.SetActive(show);
        }

        public string GetText()
        {
            return data.text;
        }

        public void Show(bool show = true)
        {
            gameObject.SetActive(show);
        }
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class DialogMenu : MonoBehaviour 
	{
        public FanMenuItem fanMenuItemTemplate;
        public float radius;
        public bool lookAtPlayer, test;

        private readonly int maxItem;
        private List<FanMenuItem> createdItems = new List<FanMenuItem>();

        private void Start()
        {
            fanMenuItemTemplate.gameObject.SetActive(false);

            if (test)
                Show();
        }

        private void Update()
        {
            if (lookAtPlayer)
                Utils.FaceCamera(transform);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show(bool show = true)
        {
            if (show)
                OpenUp();
            else
                CloseDown();
        }

        public bool Create(DialogOptions dialogOptions, System.Action<int> onSelect)
        {
            if (dialogOptions.options.Length == 0)
                return false;

            const float radStart = Mathf.PI / 2f;

            float radianIncr = Mathf.PI * 2 / dialogOptions.options.Length;

            if (createdItems.Count > 0)
            {
                Debug.Log("Items not cleared: " + createdItems.Count);

                ClearItems(false);
            }

            for (int i = 0; i < dialogOptions.options.Length; i++)
            {
                FanMenuItem fmi = GameObject.Instantiate<FanMenuItem>(fanMenuItemTemplate, transform);

                float radians = radianIncr * i;
                Vector3 circleVec = new Vector3(Mathf.Cos(radStart + radians), Mathf.Sin(radStart + radians), 0f);
                Vector3 spritePos = circleVec * radius;
                Vector3 textPos = circleVec * radius * 1.1f;
                fmi.transform.localPosition = spritePos;
                fmi.tmpText.transform.localPosition = textPos;
                fmi.Create(dialogOptions.options[i].text, i, onSelect);
                fmi.gameObject.SetActive(false);
                fmi.GetComponent<BoxCollider>().enabled = false;
                createdItems.Add(fmi);
            }

            Debug.Log("Created items, size: " + createdItems.Count);

            return true;
        }

        public bool Create(ConversationNode node, System.Action<int> onSelect)
        {
            if (node.Options.Length == 0)
                return false;

            const float radStart = Mathf.PI / 2f;

            float radianIncr = Mathf.PI * 2 / node.Options.Length;

            if (createdItems.Count > 0)
            {
                Debug.Log("Items not cleared: " + createdItems.Count);

                ClearItems(false);
            }

            for (int i = 0; i < node.Options.Length; i++)
            {
                FanMenuItem fmi = GameObject.Instantiate<FanMenuItem>(fanMenuItemTemplate, transform);

                float radians = radianIncr * i;
                Vector3 circleVec = new Vector3(Mathf.Cos(radStart + radians), Mathf.Sin(radStart + radians), 0f);
                Vector3 spritePos = circleVec * radius;
                Vector3 textPos = circleVec * radius * 1.1f;
                fmi.transform.localPosition = spritePos;
                fmi.tmpText.transform.localPosition = textPos;
                fmi.Create(node.Options[i].Text, i, onSelect);
                fmi.gameObject.SetActive(false);
                fmi.GetComponent<BoxCollider>().enabled = false;
                createdItems.Add(fmi);
            }

            Debug.Log("Created items, size: " + createdItems.Count);

            return true;
        }

        private void OpenUp()
        {
            gameObject.transform.localScale = Vector3.zero;
            foreach (FanMenuItem item in createdItems)
                item.gameObject.SetActive(true);

            gameObject.SetActive(true);

            LeanTween.scale(gameObject, Vector3.one, 1f)
            .setEaseSpring()
            .setOnComplete(() =>
            {
                foreach (FanMenuItem item in createdItems)
                    item.GetComponent<BoxCollider>().enabled = true;
            });
        }

        private void CloseDown()
        {
            // Prevent (harmless) negative collider error caused
            // by Leantween bounce effect
            foreach (FanMenuItem item in createdItems)
                item.GetComponent<BoxCollider>().enabled = false;

            LeanTween.scale(gameObject, Vector3.zero, 1f)
            .setEaseSpring()
            .setOnComplete(() =>
            {
                ClearItems(true);
            });
        }

        private void ClearItems(bool scheduled)
        {
            string status = (scheduled ? "Scheduled" : "Unscheduled");

            if (createdItems.Count > 0)
            {
                Debug.LogFormat("{0} clearing of items, size: {1}", status, createdItems.Count);

                foreach (FanMenuItem item in createdItems)
                    Destroy(item.gameObject);

                createdItems.Clear();
                gameObject.SetActive(false);
                gameObject.transform.localScale = Vector3.one;
            }
            else if (scheduled)
                Debug.LogFormat("Scheduled clearing of items, but none to clear");
        }
    }
}
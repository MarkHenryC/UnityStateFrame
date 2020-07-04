using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class FanMenu : MonoBehaviour
    {
        public FanMenuItem fanMenuItemTemplate;
        public MenuItemStore[] menuItems;
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

        public void Show(bool show = true)
        {
            if (show)
            {
                Create(menuItems.Length, radius);
                OpenUp();
            }
            else
                CloseDown();
        }

        private void Create(int itemCount, float radius)
        {
            const float radStart = Mathf.PI / 2f;

            float radianIncr = Mathf.PI * 2 / itemCount;

            for (int i = 0; i < itemCount; i++)
            {
                FanMenuItem fmi = GameObject.Instantiate<FanMenuItem>(fanMenuItemTemplate, transform);

                float radians = radianIncr * i;
                Vector3 circleVec = new Vector3(Mathf.Cos(radStart + radians), Mathf.Sin(radStart + radians), 0f);
                Vector3 spritePos = circleVec * radius;
                Vector3 textPos = circleVec * radius * 1.1f;
                fmi.transform.localPosition = spritePos;
                fmi.tmpText.transform.localPosition = textPos;
                fmi.Create(menuItems[i].tmpText, menuItems[i].spriteImage, menuItems[i].response);
                fmi.gameObject.SetActive(false);
                fmi.GetComponent<BoxCollider>().enabled = false;
                createdItems.Add(fmi);                
            }
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
                foreach (FanMenuItem item in createdItems)
                    Destroy(item.gameObject);

                createdItems.Clear();
                gameObject.SetActive(false);
                gameObject.transform.localScale = Vector3.one;
            });
        }
    }
}
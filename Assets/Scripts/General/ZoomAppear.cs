using UnityEngine;

namespace QuiteSensible
{
    public class ZoomAppear : MonoBehaviour
    {
        public bool runOnActivate;
        public Vector3 targetScale = Vector3.one;
        public float timeToFullScale = 1f;

        private Vector3 hideScale = new Vector3(0.001f, 0.001f, 0.001f);

        private void OnEnable()
        {
            if (runOnActivate)
            {
                gameObject.transform.localScale = hideScale;
                Zoom();
            }
        }

        public void Zoom()
        {
            gameObject.SetActive(true);
            LeanTween.scale(gameObject, targetScale, timeToFullScale)
                .setEaseOutBounce();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            gameObject.transform.localScale = hideScale;
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace QS
{
    public class SceneLoader : MonoBehaviour
    {
        public UnityEvent LoadEvent;
        public UnityEvent UnloadEvent;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadScene(string name)
        {
            LoadEvent?.Invoke();

            Debug.Log("Loading scene with fade");

            ActivityManager.Instance.FadeOutThen(() =>
                { SceneManager.LoadScene(name); });
        }

        public void LoadScene(string name, bool fade)
        {
            LoadEvent?.Invoke();

            if (fade)
            {
                Debug.LogFormat("LoadScene: calling SceneManager.LoadScene({0}) with with fade via FadeOutThen", name);

                ActivityManager.Instance.FadeOutThen(() =>
                    { SceneManager.LoadScene(name); });
            }
            else
            {
                Debug.LogFormat("LoadScene: calling SceneManager.LoadScene({0}) with no fade", name);

                SceneManager.LoadScene(name);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            UnloadEvent?.Invoke();
        }
    }
}
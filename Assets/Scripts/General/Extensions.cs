using UnityEngine;
using System.Collections.Generic;

namespace QS
{
    public static class Extensions
    {
        public static bool IsNamed(this Graspable g, string name)
        {
            return (g && g.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool IsTagged(this Graspable g, string tag)
        {
            return (g && g.CompareTag(tag));
        }

        public static bool IsEqualTo(this Graspable g, GameObject other)
        {
            return (g && g.gameObject.Equals(other));
        }

        public static bool IsA<T>(this Graspable g)
        {
            if (g)
                return (g.GetComponent<T>() != null);
            else
                return false;
        }

        public static T GetA<T>(this Graspable g)
        {
            if (g)
                return g.GetComponent<T>();
            else
                return default;
        }

        public static bool IsNamed(this Receivable r, string name)
        {
            return (r && r.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool Usable(this string str)
        {
            return !Useless(str);
        }

        public static bool Useless(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string After(this string entire, string prefix)
        {
            return entire.Remove(0, prefix.Length);
        }

        public static void AddTo(this Dictionary<string, int> map, string key, int i = 1)
        {
            if (map.ContainsKey(key))
                i += map[key];

            map[key] = i;
        }

        public static void SetMode(this GameObject obj, bool on)
        {
            if (obj)
            {
                if (on && !obj.activeSelf)
                    obj.SetActive(true);
                else if (!on && obj.activeSelf)
                    obj.SetActive(false);
            }
        }

        public static void SetMode(this AudioSource audio, bool play)
        {
            if (audio)
            {
                if (play && !audio.isPlaying)
                {
                    Debug.Log("Triggering PLAY");
                    audio.Play();
                }
                else if (!play && audio.isPlaying)
                {
                    Debug.Log("STOPPING play");
                    audio.Stop();
                }
            }
        }

        public static GameObject ActiveGameObject(this Graspable g)
        {
            return g ? g.gameObject : null;
        }

        public static void Set(this Transform target, Transform source)
        {
            if (target && source)
            {
                target.position = source.position;
                target.rotation = source.rotation;
            }
        }

        public static void PointToPlayer(this Transform target)
        {
            if (target)
                target.LookAt(ControllerInput.Instance.Player);
        }

        public static void Set(this Transform target, Vector3 worldPos)
        {
            if (target)
                target.position = worldPos;
        }

        public static Color NormalizedOpaque(this Color c)
        {
            float max = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
            return new Color(c.r / max, c.g / max, c.b / max, 1f);
        }

        public static float DeviationFromYellow(this Color c)
        {
            return (Mathf.Max(c.r, c.g) - Mathf.Min(c.r, c.g)) + c.b;
        }

        public static float DeviationFromMagenta(this Color c)
        {
            return (Mathf.Max(c.r, c.b) - Mathf.Min(c.r, c.b)) + c.g;
        }

        public static Vector3 AbsoluteLerp(this Vector3 start, Vector3 end, float subtract)
        {
            Vector3 direction = (end - start).normalized;
            return start + direction * (Vector3.Distance(start, end) - subtract);
        }

        public static bool CloseEnough(this Vector3 val1, Vector3 val2)
        {
            return 
                Mathf.Abs(val1.x - val2.x) < .05f &&
                Mathf.Abs(val1.y - val2.y) < .05f &&
                Mathf.Abs(val1.z - val2.z) < .05f;
        }

        /// <summary>
        /// Camera-based
        /// </summary>
        /// <param name="lookDir"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        public static bool LookingAwayFrom(this Vector3 targetPos)
        {
            return Vector3.Dot(Camera.main.transform.forward, targetPos - Camera.main.transform.position) < 0f;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                        Debug.LogException(new System.Exception("Scene corrupt! No object of type " + typeof(T).ToString()));
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (!Application.isPlaying)
                return;

            _instance = this as T;
        }
    }
}
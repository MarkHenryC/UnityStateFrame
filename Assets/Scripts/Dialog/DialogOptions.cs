using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    [CreateAssetMenu()]
    public class DialogOptions : ScriptableObject
	{
        public Action<EntryAction> OnEnter;
        public Action<SelectionOptions[]> OnMenu;

        [Serializable]
        public class EntryAction
        {
            public string text;
            public AudioClip voice;
            public string action; // Such as an animation trigger

            public static implicit operator bool(EntryAction e)
            {
                return e != null;
            }
        }

        public EntryAction entryAction;

        [Serializable]
        public class SelectionOptions
        {
            public string text;
            public int points;
            public DialogOptions destination;

            public static implicit operator UnityEngine.Object(SelectionOptions v)
            {
                throw new NotImplementedException();
            }
        }

        public SelectionOptions[] options;

        public void Enter()
        {
            if (entryAction)            
                OnEnter?.Invoke(entryAction);

            OnMenu?.Invoke(options);
        }
    }
}
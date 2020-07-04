using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;

namespace QS
{
    [CreateAssetMenu()]
    public class MenuItemData : ScriptableObject
    {
        public Sprite spriteImage;
        public string tmpText;
        public UnityEvent response;
        public Action<Graspable> onSelect;
    }    
}
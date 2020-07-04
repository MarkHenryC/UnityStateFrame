using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace QS
{
    [Serializable]
    public class MenuItemStore
    {
        public Sprite spriteImage;
        public string tmpText;
        public UnityEvent response;
    }
}
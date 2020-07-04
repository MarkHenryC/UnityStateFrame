using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    [CreateAssetMenu()]
    public class SharedFloat : ScriptableObject
    {
        [NonSerialized]
        public float Value;
    }    
}
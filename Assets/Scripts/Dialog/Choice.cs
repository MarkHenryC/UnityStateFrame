using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    [CreateAssetMenu()]
    public class Choice : ScriptableObject 
	{
        public string text;
        public int points;
        public int destination;
	}
}
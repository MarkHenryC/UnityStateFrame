using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class PersistAcrossScenes : MonoBehaviour 
	{
		void Awake () 
		{
            DontDestroyOnLoad(this);
		}
		
		void Start () 
		{
			
		}	
	}
}
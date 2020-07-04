using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    /// <summary>
    /// Derivatives of this class handle specifics such as raycasting
    /// </summary>
    public abstract class InputProcessor : MonoBehaviour
    {
        protected Camera mainCam;

        public virtual void Awake()
        {
            mainCam = Camera.main;
        }

        public abstract VrEventInfo ProcessController(VrEventInfo info);
    }
}
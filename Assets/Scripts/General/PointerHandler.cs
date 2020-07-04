using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{    
    public abstract class PointerHandler : MonoBehaviour
    {
        [Tooltip("Standard beam pointer")]
        public PointerFeedback beamPointer;
        [Tooltip("Default raycaster that simply updates VrEventInfo")]
        public InputProcessor standardRaycaster;
        [Tooltip("Default raycast action dispatcher. Uses results from standardRaycaster")]
        public InteractionDispatcherBase interactionDispatcher;

        public abstract void Activate(bool on);
        public abstract void Handle(VrEventInfo info);        
    }
}
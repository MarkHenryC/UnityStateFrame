using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class PointerHandlerHolding : PointerHandler
    {
        public override void Activate(bool on)
        {
            beamPointer.Activate(on);
        }

        public override void Handle(VrEventInfo info)
        {
            if (interactionDispatcher)
                info = interactionDispatcher.DispatchInteraction(info);
            if (beamPointer)
                beamPointer.OnFrame(info);
        }
    }
}

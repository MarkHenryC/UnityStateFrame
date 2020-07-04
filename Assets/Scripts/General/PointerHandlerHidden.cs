using UnityEngine;

namespace QS
{
    public class PointerHandlerHidden : PointerHandler
    {
        public override void Activate(bool on)
        {
            beamPointer.Activate(!on);
        }

        public override void Handle(VrEventInfo info)
        {
        }
    }

}
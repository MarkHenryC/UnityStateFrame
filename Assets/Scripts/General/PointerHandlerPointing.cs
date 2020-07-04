namespace QS
{
    public class PointerHandlerPointing : PointerHandler
    {
        public override void Activate(bool on)
        {
            beamPointer.Activate(on);
        }

        public override void Handle(VrEventInfo info)
        {
            if (standardRaycaster)
                info = standardRaycaster.ProcessController(info);
            if (interactionDispatcher)
                info = interactionDispatcher.DispatchInteraction(info);
            if (beamPointer)
                beamPointer.OnFrame(info);
        }
    }

}
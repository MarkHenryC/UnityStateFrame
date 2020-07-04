using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
	public class StateTestBegin : StateProcessor 
	{
		[TextArea]
        public string startPrompt;
		public string continuePrompt;    
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public float time;

        private float counter;
        private bool complete;

        public override void Enter(ActivityBase a, StateProcessor previousState)
        {
            base.Enter(a, previousState);

            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Pointing;

			if (infoPanel)
				infoPanel.SetText(startPrompt);

			if (continueButton)
				continueButton.Activate(continuePrompt, (continueButton) =>
				{
					ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(); });
				});

        }

        public override void Exit()
        {
            base.Exit();
			if (infoPanel)
				infoPanel.Show(false);
			if (continueButton)
				continueButton.Show(false);
        }

        public override void OnFrame(VrEventInfo processedVrEventInfo)
        {
            
            base.OnFrame(processedVrEventInfo);

            switch (processedVrEventInfo.EventType)
            {
                case VrEventInfo.VrEventType.TriggerDown:
                    break;
                default:
                    break;
            }
        }
	
	}
}
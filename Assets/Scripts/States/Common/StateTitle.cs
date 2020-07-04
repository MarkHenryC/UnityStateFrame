using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
	public class StateTitle : StateProcessor 
	{
        public string startPrompt, continuePrompt;    
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public AudioClip voiceover;

        public override void Enter(ActivityBase a, StateProcessor previousState)
        {
            base.Enter(a, previousState);

            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.None;
            
            LeanTween.delayedCall(ActivitySettings.Asset.titleDisplayTime, () =>
            {
                ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(); });
            });
        
            Camera.main.clearFlags = CameraClearFlags.SolidColor;

            if (voiceover)
                ControllerInput.Instance.PlayVoiceover(voiceover);
        }

        public override void Exit()
        {
            base.Exit();
			if (infoPanel)
				infoPanel.Show(false);
			if (continueButton)
				continueButton.Show(false);
        }
	}
}
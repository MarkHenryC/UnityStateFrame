using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
	public class StateSaladIntro : StateProcessor 
	{
        public string startPrompt, continuePrompt;    
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;

        public override void Enter(ActivityBase a, StateProcessor previousState)
        {
            base.Enter(a, previousState);

            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.None;

			if (infoPanel)
				infoPanel.SetText(startPrompt);

            // ContinueButton action can be set here, but for convenience for non-programmers
            // it can also be set with a UnityEvent on the activity's scene 
            // content folder (in this case ChallengeObjects) Continue button.
            // For the last state in the group, drag the ActivityManager scene object, which contains a 
            // SceneLoader component, to the UnityEvent and select SceneLoader/LoadScene
            // and set the text to Start.

            LeanTween.delayedCall(ActivitySettings.Asset.titleDisplayTime, () =>
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
	}
}
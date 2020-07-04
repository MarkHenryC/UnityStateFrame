using UnityEngine;

namespace QS
{
    public class StateSimpleSwitchBegin : StateProcessor
    {
        [TextArea]
        public string startPrompt;
        public string continuePrompt;
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public bool introOnly;
        public bool dismissOnAction;
        public Circuit circuit;
        public Block device1, device2;
        public Switch switch1;

        private bool started, finished;
        private int shortCount, closedCount;
        private bool testComplete;
        private Circuit.CircuitType circuitState;
        private SwitchTest switch1Test = new SwitchTest("Switch1");
        private bool directWired;

        public override void Enter(ActivityBase a, StateProcessor previousState)
        {
            base.Enter(a, previousState);

            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Pointing;

            if (infoPanel)
            {
                infoPanel.SetText(string.Format(startPrompt, timeToCompleteActivity));

                if (introOnly)
                {
                    ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.None;

                    if (!clickToContinue)
                        infoPanel.ShowFor(ActivitySettings.Asset.TextDisplayTime(startPrompt), () =>
                        {
                            PostExit(true);
                        });
                }
            }

            if (continueButton)
                continueButton.Activate(continuePrompt, (continueButton) =>
                {
                    ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(); });
                });

            switch1.NotifyOnSwitch = Switched1;
            switch1.gameObject.SetActive(false);
            device1.OnConnectionChanged = OnConnectionChanged;
            device2.OnConnectionChanged = OnConnectionChanged;
            circuitState = Circuit.CircuitType.Open;
            circuit.CallOnTraceComplete = OnTraceComplete;
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

            if (finished)
                return;

            switch (processedVrEventInfo.EventType)
            {
                case VrEventInfo.VrEventType.TriggerDown:
                case VrEventInfo.VrEventType.TouchpadClickDown:
                    if (!started)
                    {
                        started = true;
                        if (dismissOnAction)
                        {
                            if (infoPanel && !introOnly)
                                infoPanel.Show(false);
                        }
                        if (timeToCompleteActivity > 0f)
                            StartTimer();
                    }
                    break;
            }
        }

        protected override void OnTimedOut()
        {
            if (!finished)
            {
                base.OnTimedOut();
                Finish();
            }
        }

        private void Finish()
        {
            finished = true;
            DisableTimer();

            string report = "";

            int points = testComplete ? ActivitySettings.pointsPerChallenge : 0;
            if (shortCount > 0)
                report += "A bit of smoke there. ";
            else
                points += 5;

            if (testComplete)
                report += "Well done. No short circuits. ";
            else if (directWired)
                report += "Made the first connection. Points for no short circuits. ";
            else
            {
                report += "No connections made unfortunately. ";
                points = 0;
            }

            Utils.RegisterActivityAndUpdateExperience(points);

            infoPanel.SetText(report + "Points earned: " + ActivitySettings.Asset.currentActivityScore);
            infoPanel.TryBonus();

            infoPanel.ShowFor(ActivitySettings.Asset.titleDisplayTime, () => { PostExit(true); });
        }

        private void OnTraceComplete(Circuit.CircuitType ct, Circuit.TraceTrigger trigger)
        {
            Circuit.CircuitType previousState = circuitState;
            circuitState = ct;
            bool changedState = (previousState != circuitState) &&
                                (previousState != Circuit.CircuitType.Short) &&
                                (circuitState != Circuit.CircuitType.Short);

            switch (circuitState)
            {
                case Circuit.CircuitType.Closed:
                    closedCount++;
                    infoPanel.SetText("Light on");
                    if (!directWired)
                    {
                        var ps = device1 as PowerSource;
                        ps.live.ClearOutgoingConnection();
                        ps.neutral.ClearOutgoingConnection();
                        directWired = true;
                        switch1.gameObject.SetActive(true);
                        PlayReward();
                        infoPanel.SetText("Great. Now let's try it with a switch. Run one of the cables through the switch and test it off and on.");
                    }
                    break;
                case Circuit.CircuitType.Short:
                    infoPanel.SetText("Short circuit!");
                    shortCount++;
                    break;
                case Circuit.CircuitType.Open:
                    infoPanel.SetText("Light off");
                    break;
            }

            if (trigger == Circuit.TraceTrigger.Rewire || circuitState == Circuit.CircuitType.Short)
            {
                Debug.Log("Reset after short or rewire");

                switch1Test.ResetTests();
            }
            else if (trigger == Circuit.TraceTrigger.Switch)
            {
                if (changedState)
                {
                    if (switch1Test.actionPending)
                    {
                        switch1Test.TestStatus(switch1.IsUp, circuitState == Circuit.CircuitType.Open);
                    }
                }
                else // if we've flicked a switch and no change then it's wired incorrectly
                {
                    switch1Test.ResetTests();
                }

                if (switch1Test.SingleSwitchTestComplete)
                {
                    testComplete = true;

                    PlayReward();

                    Finish();

                    Debug.Log("Testing successfully completed");
                }
            }

            switch1Test.actionPending = false;

        }

        private void OnConnectionChanged(Connection target)
        {
            Debug.Log("Changed connection on: " + target.name);
        }

        private void Switched1(bool up, Switchable source)
        {
            switch1Test.actionPending = true;

            Debug.Log("Switched 1 to " + (up ? "up" : "down"));
        }

    }
}
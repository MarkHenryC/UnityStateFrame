using UnityEngine;

namespace QS
{
    public class StateConnectLightSwitchesBegin : StateProcessor
    {
        [TextArea]
        public string startPrompt;
        public string continuePrompt;
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public Circuit circuit;
        public GameObject shortCircuit; // SpitzenSparkz
        public Switch switch1, switch2;
        public Block[] blockComponents; // so we can monitor changes for later testing
        public AudioClip voiceover;

        private bool complete;
        private int shortCount, closedCount;
        private bool testComplete;
        private Circuit.CircuitType circuitState;
        private SwitchTest switch1Test = new SwitchTest("Switch1");
        private SwitchTest switch2Test = new SwitchTest("Switch2");
        private bool switchedOn1, switchedOn2; // Late hack to simplify player's testing

        private const int ShortMultiplier = 5;

        public override void Enter(ActivityBase a, StateProcessor previousState)
        {
            base.Enter(a, previousState);

            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Pointing;

            if (infoPanel)
                infoPanel.SetText(string.Format(startPrompt, Utils.Minutised(timeToCompleteActivity)));

            if (continueButton)
                continueButton.Activate(continuePrompt, (continueButton) =>
                {
                    ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(); });
                });

            switch1.NotifyOnSwitch = Switched1;
            switch2.NotifyOnSwitch = Switched2;

            circuitState = Circuit.CircuitType.Open;
            circuit.CallOnTraceComplete = OnTraceComplete;

            foreach (Block b in blockComponents)
                b.OnConnectionChanged = ConnectionUpdated;

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

        public override void OnFrame(VrEventInfo processedVrEventInfo)
        {
            base.OnFrame(processedVrEventInfo);

            if (complete)
                return;

            switch (processedVrEventInfo.EventType)
            {
                case VrEventInfo.VrEventType.TriggerDown:
                case VrEventInfo.VrEventType.TouchpadClickDown:
                    StartTimer();
                    break;
            }
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
                    if (infoPanel)
                        infoPanel.SetText("Light on");
                    break;
                case Circuit.CircuitType.Short:
                    if (infoPanel)
                        infoPanel.SetText("Short circuit!");
                    shortCount++;
                    break;
                case Circuit.CircuitType.Open:
                    if (infoPanel)
                        infoPanel.SetText("Light off");
                    break;
            }

            if (trigger == Circuit.TraceTrigger.Rewire || circuitState == Circuit.CircuitType.Short)
            {
                Debug.Log("Reset after short or rewire");

                switch1Test.ResetTests();
                switch2Test.ResetTests();
            }
            else if (trigger == Circuit.TraceTrigger.Switch)
            {
                if (changedState)
                {
                    if (switch1Test.actionPending || switch2Test.actionPending)
                    {
                        if (switch1Test.actionPending && circuitState == Circuit.CircuitType.Closed)
                            switchedOn1 = true;
                        else if (switch2Test.actionPending && circuitState == Circuit.CircuitType.Closed)
                            switchedOn2 = true;

                        switch1Test.TestStatus(switch1.IsUp, circuitState == Circuit.CircuitType.Open);
                        switch2Test.TestStatus(switch2.IsUp, circuitState == Circuit.CircuitType.Open);
                    }
                }
                else // if we've flicked a switch and no change then it's wired incorrectly
                {
                    switch1Test.ResetTests();
                    switch2Test.ResetTests();
                }

                if (switch1Test.TestComplete && switch2Test.TestComplete || (switchedOn1 && switchedOn2))
                {
                    testComplete = true;
                    complete = true;

                    Finish();

                    Debug.Log("Testing successfully completed");
                }
            }

            switch1Test.actionPending = switch2Test.actionPending = false;
        }

        protected override void OnTimedOut()
        {
            if (!complete)
            {
                base.OnTimedOut();
                complete = true;
                Finish();
            }
        }

        private void Finish()
        {
            int points = 0;
            int deductions = shortCount * ShortMultiplier;

            DisableTimer();

            string report = "";

            if (testComplete)
            {
                report += "Testing successfully completed. ";

                points = ActivitySettings.pointsPerChallenge - deductions;
                if (shortCount < 3)
                {
                    if (shortCount == 0)
                    {
                        PlayReward();
                        report += "Bonus for no short circuits! ";
                    }
                    else
                        report += string.Format("Partial bonus. Short circuit count: {0}. ", shortCount);

                    points += ActivitySettings.pointsPerChallenge - deductions;
                }
                else
                    report += string.Format("Correct final result, but unfortunately {0} short circuits. ", shortCount);
            }
            else if (switch1Test.TestComplete || switch2Test.TestComplete || closedCount > 0)
            {
                points = (ActivitySettings.pointsPerChallenge / 3) - deductions;
                report += "Got one switch working correctly, which is a start. ";
                if (shortCount > 0)
                {
                    if (shortCount > 1)
                        report += string.Format("But there were {0} short circuits. ", shortCount);
                    else
                        report += "But there was a short circuit. ";
                }
            }
            else
            {
                report += "Not much to report here. No connections made. ";
                if (shortCount > 0)
                {
                    if (shortCount > 1)
                        report += string.Format("But you did manage {0} short circuits. ", shortCount);
                    else
                        report += "But you did manage a short circuit. ";
                }
            }

            Utils.RegisterActivityAndUpdateExperience(points);

            if (infoPanel)
            {
                infoPanel.SetText(report + "Points earned: " + ActivitySettings.Asset.currentActivityScore);
                infoPanel.TryBonus();

                infoPanel.ShowFor(ActivitySettings.Asset.titleDisplayTime, () => { PostExit(true); });
            }
        }

        private void Switched1(bool up, Switchable source)
        {
            switch1Test.actionPending = true;

            Debug.Log("Switched 1 to " + (up ? "up" : "down"));
        }

        private void Switched2(bool up, Switchable source)
        {
            switch2Test.actionPending = true;

            Debug.Log("Switched 2 to " + (up ? "up" : "down"));
        }

        private void ConnectionUpdated(Connection c)
        {            
            Debug.Log("Updated connection");
        }
    }
}
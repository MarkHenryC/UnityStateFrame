using System;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class StateConnectBatteriesBegin : StateProcessor
    {
        [TextArea]
        public string startPrompt;
        public string continuePrompt;
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public Cable cableTemplate;
        public Transform connections; // container for instantiated connections
        public ClickableObject[] selectables;
        public AudioClip voiceover;
        public GameObject shortCircuitFx;

        private ClickableObject selected;
        private Cable newCable;
        private float beamLengthAtTerminal; // For calculating the end point of the cable before completing connection
        private int correctConnections, incorrectConnections;
        private bool shortCircuit;
        private bool complete;

        private const int CORRECT_COUNT = 5;
        private const int INCORRECT_MULTIPLIER = 5;

        public class ConnectionTerminal : IComparable
        {
            public int batteryId;
            public bool isPositive;

            public int CompareTo(object otherObj)
            {
                // Probably don't need terminal charge sub-sorting
                // as we're not registering connections where a
                // given battery's terminals are connected to each other (aka spitzensparkz)

                if (!(otherObj is ConnectionTerminal other))
                    return 1;

                if (batteryId == other.batteryId)
                {
                    if (isPositive)
                    {
                        if (other.isPositive)
                            return 0;
                        else
                            return -1; // Higher sort for positive
                    }
                    else if (other.isPositive)
                        return 1;
                    else
                        return 0;
                }
                else if (batteryId < other.batteryId)
                    return -1; // higher sort for lower number
                else
                    return 1;
            }
        }

        public class Connected : IComparable, IEquatable<Connected>
        {
            public ConnectionTerminal t1, t2;

            public Cable connection;

            public Connected(int bId1, bool isP1, int bId2, bool isP2, Cable visual = null)
            {
                if (bId1 < bId2)
                {
                    t1 = new ConnectionTerminal { batteryId = bId1, isPositive = isP1 };
                    t2 = new ConnectionTerminal { batteryId = bId2, isPositive = isP2 };
                }
                else if (bId1 > bId2)
                {
                    t2 = new ConnectionTerminal { batteryId = bId1, isPositive = isP1 };
                    t1 = new ConnectionTerminal { batteryId = bId2, isPositive = isP2 };
                }
                else
                    Debug.LogException(new Exception("Adding connection with both terminals on same battery"));

                connection = visual;
            }

            // For the sake of comparing connections we're only interested in the battery number
            // as a connection on the same battery ID would be a short circuit
            public int CompareTo(object otherObj)
            {
                if (!(otherObj is Connected other))
                    return -1;

                int compareT1 = t1.CompareTo(other.t1);
                if (compareT1 == 0)
                    return t2.CompareTo(other.t2);
                else
                    return compareT1;
            }

            public bool Equals(Connected other)
            {
                return t1.CompareTo(other.t1) == 0 && t2.CompareTo(other.t2) == 0;
            }
        }

        private List<Connected> connectedList = new List<Connected>();
        private List<Connected> correctConnectionList = new List<Connected>();

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

            foreach (ClickableObject s in selectables)
            {
                s.OnClick = Select;
                s.PointerEnter = OnTerminalPointerEnter;
                s.PointerExit = OnTerminalPointerExit;
            }

            SetupCorrectList();

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

            switch (processedVrEventInfo.EventType)
            {
                case VrEventInfo.VrEventType.TriggerDown:
                    break;
            }

            if (newCable)
            {
                Vector3 cableDest = ControllerInput.Instance.ControllerPosition +
                    ControllerInput.Instance.ControllerDirection * beamLengthAtTerminal;
                newCable.UpdateConnection(cableDest);
            }
        }

        public void Test()
        {
            if (!complete)
            {
                correctConnections = 0;
                foreach (Connected connected in correctConnectionList)
                {
                    if (connectedList.Contains(connected))
                    {
                        correctConnections++;
                        connectedList.Remove(connected);
                    }
                }

                incorrectConnections = connectedList.Count; // any left over are wrong

                Finish();
            }
        }

        public void Clear(string dummy)
        {
            if (complete)
                return;

            foreach (Connected c in connectedList)
            {
                if (c.connection)
                    Destroy(c.connection.gameObject);
            }
            connectedList.Clear();

            // A bit of a temp hack. Connections are not
            // being added to the connectedList if they
            // have the same endpoints but are created
            // anyway, leaving orphans. TODO: fix.
            // Meanwhile, this clears all visibles
            for (int i = 0; i < connections.childCount; i++)
                Destroy(connections.GetChild(i).gameObject);
        }

        private void OnTerminalPointerEnter(ClickableObject c)
        {
            if (newCable)
            {
                newCable.UpdateConnection(c.transform.position);
                beamLengthAtTerminal = Vector3.Distance(c.transform.position, ControllerInput.Instance.ControllerPosition);
            }
        }

        private void OnTerminalPointerExit(ClickableObject c)
        {
        }

        private void Select(ClickableObject obj)
        {
            ClickableObject prevSelected = selected;

            selected = obj;

            if (selected)
            {
                if (prevSelected)
                {
                    if (prevSelected == selected)
                    {
                        Destroy(newCable);
                        newCable = null;
                    }
                    else
                        MakeConnection(prevSelected, selected);

                    prevSelected = selected = null;
                }
                else
                {
                    // Start of connection
                    newCable = Instantiate(cableTemplate, connections) as Cable;
                    newCable.gameObject.SetActive(true);
                    newCable.objectFrom = obj.transform;

                    beamLengthAtTerminal = Vector3.Distance(obj.transform.position, ControllerInput.Instance.ControllerPosition);
                }
            }

            foreach (ClickableObject s in selectables)
                s.Select(s == selected);
        }

        private void MakeConnection(ClickableObject aOb, ClickableObject bOb)
        {
            Debug.Assert(newCable.objectFrom == aOb.transform, "MakeConnection: start connector doesn't match");

            newCable.objectTo = bOb.transform;
            newCable.UpdateConnection();

            bool shortCircuit = false, madeConnection = false;

            if (int.TryParse(aOb.transform.parent.name, out int idA) &&
                int.TryParse(bOb.transform.parent.name, out int idB))
            {
                bool isP1 = (aOb.name == "TerminalPositive");
                bool isP2 = (bOb.name == "TerminalPositive");

                madeConnection = true;

                if (idA != idB)
                {
                    Connected newCon = new Connected(idA, isP1, idB, isP2, newCable);
                    if (!connectedList.Contains(newCon))
                        connectedList.Add(newCon);
                    // Otherwise let it dangle
                }
                else
                {
                    shortCircuit = true; // Same battery means short
                    shortCircuitFx.SetActive(true);
                    Destroy(newCable);
                }
                connectedList.Sort();
            }
            else
                Debug.LogError("Evaluating connectors without valid parent IDs convertable to integers");

            if (madeConnection) // only if connection made, otherwise continue to look for endpoint
                newCable = null;

            if (shortCircuit)
                ShortCircuit(idA);
        }

        private void ShortCircuit(int batteryId)
        {
            Debug.Log("Short circuit!!!");

            shortCircuit = true;
            Finish();
        }

        private void Finish()
        {
            complete = true;

            string report = "";

            int total = correctConnections;
            if (total == CORRECT_COUNT)
                total *= correctConnections;

            total -= (incorrectConnections * INCORRECT_MULTIPLIER);

            if (shortCircuit)
            {
                total = 0;
                report += "Short circuit! ";
            }

            if (correctConnections == CORRECT_COUNT && incorrectConnections == 0)
                PlayReward();

            Utils.RegisterActivityAndUpdateExperience(total);

            report += string.Format("Correct connections: {0} out of {1}. Incorrect connections: {2}. Total points: {3}",
                correctConnections, correctConnectionList.Count, incorrectConnections, ActivitySettings.Asset.currentActivityScore);

            infoPanel.SetText(report);
            infoPanel.TryBonus();

            infoPanel.ShowFor(ActivitySettings.Asset.titleDisplayTime, () => { PostExit(true); });
        }

        private void SetupCorrectList()
        {
            correctConnectionList.Add(new Connected(1, true, 2, true));
            correctConnectionList.Add(new Connected(3, true, 4, true));
            correctConnectionList.Add(new Connected(1, false, 2, false));
            correctConnectionList.Add(new Connected(3, false, 4, false));
            correctConnectionList.Add(new Connected(2, true, 3, false));

            correctConnectionList.Sort();
        }
    }
}
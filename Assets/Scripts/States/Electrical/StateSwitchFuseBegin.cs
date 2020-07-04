using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class StateSwitchFuseBegin : StateProcessor
    {
        [TextArea]
        public string startPrompt;
        public string continuePrompt;
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public Switchable[] fuses;
        public int correctFuseToSwitchOff;
        public GameObject spitznsparkz; // Special German word for short circuit
        public InfoPanel storeRoomInfoPanel;
        public AudioClip scream;
        public AudioClip voiceover;
        public NpcController electrician;
        public Transform electricianStartPosition;
        public Transform[] electricianPath;

        private bool complete, catastrophicFail;
        private List<int> selectedSwitches = new List<int>();
        private readonly string[] fuseMarkers = new[] { "1", "2", "3", "#", "5", "6", "_", "*", "~", "10", "11", "12", "#", "=", "" };
        private const int deductForLabelledFuse = 10, deductForDownFuse = 15, deductForSequenceFuse = 2;

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

            int ix = 0;
            foreach (Switchable v in fuses)
            {
                v.CallOnSwitchId = OnSwitch;
                v.SetLabel(fuseMarkers[ix++]);
            }

            spitznsparkz.SetActive(true);

            electrician.Talk(voiceover);
            electrician.SetAnimation("Walk");
            electrician.restingAnim = "Idle";
            electrician.transform.Set(electricianStartPosition);
            electrician.MoveAlongPathThen(electricianPath, 3f, "Idle");

            //ControllerInput.Instance.PlayVoiceover(voiceover);
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
                case VrEventInfo.VrEventType.TouchpadClickDown:
                    StartTimer();
                    break;
            }
        }

        private void OnSwitch(bool up, Switchable source)
        {
            // We know these are 1-based
            if (int.TryParse(source.name, out int id))
            {
                if (!selectedSwitches.Contains(id))
                    selectedSwitches.Add(id);

                if (!up)
                {
                    if (id == correctFuseToSwitchOff)
                    {
                        spitznsparkz.SetActive(false);
                        Finish();
                    }
                }
                else if (id == 11 || id == 13)
                {
                    catastrophicFail = true;
                    spitznsparkz.SetActive(true);
                    LeanAudio.playClipAt(scream, spitznsparkz.transform.position);
                    Finish();
                }
            }
        }

        protected override void OnTimedOut()
        {
            if (!complete)
            {
                base.OnTimedOut();                
                Finish();
            }
        }

        private void Finish()
        {
            int deductions = 0;
            int bonus = 0, score = 0;
            string response = "";

            DisableTimer();

            complete = true;

            if (catastrophicFail)
            {
                response = "You activated a fuse that was off! Very dangerous. Someone may be working on that circuit. ";
            }
            else
            {
                bool gotResult = selectedSwitches.Contains(correctFuseToSwitchOff);
                if (gotResult)
                {
                    response += string.Format("Fuse {0} is correct. ", correctFuseToSwitchOff);
                    if (selectedSwitches.Count == 1)
                    {
                        response += "Sensational! Did it first try. Sorry about the damaged fuse chart, but you worked around it. ";
                        PlayReward();
                    }
                    else
                        response += "Faulty circuit shut down. Summary of search for BB power point fuse: ";
                }

                foreach (var s in selectedSwitches)
                {
                    if ((s >= 1 && s <= 6) || s == 10 || s == 12 || s == 14 || s == 15)
                    {
                        response += string.Format("Fuse {0} is clearly marked, and doesn't match. ", s);
                        deductions += deductForLabelledFuse;
                    }
                    else if (s == 11 || s == 13)
                    {
                        response += string.Format("It couldn't be fuse {0}, as it was down while the powerpoint was sparking. What's more, it's very dangerous closing an unknown fuse! ", s);
                        deductions += deductForDownFuse;
                    }
                    else if (s == 7 || s == 9)
                    {
                        response += string.Format("Fuse {0} is in the part of a clear sequence in the chart, and doesn't match BB. ", s);
                        deductions += deductForSequenceFuse;
                    }
                }                

                if (gotResult)
                {
                    score = ActivitySettings.pointsPerChallenge - deductions;
                    if (deductions < 5)
                        bonus = ActivitySettings.pointsPerChallenge - deductions;
                }                
            }
            Utils.RegisterActivityAndUpdateExperience(score + bonus);

            response += "Points: " + ActivitySettings.Asset.currentActivityScore;

            storeRoomInfoPanel.SetText(response);
            storeRoomInfoPanel.TryBonus();

            infoPanel.SetText(response);
            infoPanel.TryBonus();

            infoPanel.ShowFor(ActivitySettings.Asset.titleDisplayTime, () => { PostExit(true); });
        }
    }
}
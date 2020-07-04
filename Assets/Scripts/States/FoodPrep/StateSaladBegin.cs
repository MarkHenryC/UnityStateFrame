using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class StateSaladBegin : StateProcessor
    {
        public DialHandler dialHandler;
        public Transform ingredientDestination;
        public Placeable[] placeables;
        public Pourable[] pourables;
        public Sliceable[] sliceables;
        public Recipe saladRecipe;
        public InfoPanel infoPanel;
        public ButtonPanel continueButton;
        public string scoreText, continueText;
        public Twistable controlKnob;
        public ButtonPanel addSlicesButton;
        public InfoPanel dropInfo;
        public FanMenu eggPrep, baguetteSlicesPrep, grilledBaguetteSlicesPrep, garlicPrep, lettucePrep;
        public Animator chefAnimatorController;
        public GameObject chefEvaluation;
        public ExpressionHandler chefExpression;

        private Dictionary<string, int> addedItems = new Dictionary<string, int>();
        private Dictionary<string, List<string>> prepSteps = new Dictionary<string, List<string>>();
        private Sliceable currrentSliceTarget;
        private bool activeMenu; // quick hack for now
        private bool pouredLiquid, pouredGranular, droppedEgg, droppedBaguette, droppedGarlic, droppedLettuce;
        private bool scored;
        private bool complete;

        public override void Enter(ActivityBase a, StateProcessor previousState)
        {
            base.Enter(a, previousState);

            ControllerInput.Instance.PointerMode = ControllerInput.EnPointerMode.Pointing;

            foreach (var p in placeables)
            {
                p.CallOnDrop = OnPlaceableDrop;
                if (!addedItems.ContainsKey(p.name))
                    addedItems[p.name] = 0;
            }

            foreach (var r in pourables)
            {
                r.CallOnDrop = OnPourDrop;
                r.Pouring = OnPouring;
                r.CallOnEnter = OnEnterZone;
                r.CallOnExit = OnExitZone;
                r.PouringComplete = OnFinishedPouring;
            }

            foreach (var s in sliceables)
            {
                s.CallOnDrop = OnSliceableDrop;
            }

            addSlicesButton.Show(false);
            addSlicesButton.clickUpAction = AddSlices;
            infoPanel.Show(false);
            dropInfo.Show(false);
            controlKnob.Show(false);

            Utils.OptSkipButton("Skip salad preparation", continueButton);

            ControllerInput.Instance.LockPlayer(false);

            chefEvaluation.SetActive(false);
            chefExpression.Init();
            chefExpression.Neutral();
        }

        public override void Exit()
        {
            base.Exit();
            infoPanel.Show(false);
            continueButton.Show(false);
            ControllerInput.Instance.LockPlayer(true);
        }

        public override void OnSlowFrame(VrEventInfo processedVrEventInfo)
        {
            if (complete)
                return;

            if (Done() && !scored)
            {
                scored = true;
                complete = true;
                continueButton.Activate(scoreText, BringInChef);
            }
        }

        public void EndLooking()
        {
            Score(null);
        }

        public void OnInstructionsHidden()
        {
            continueButton.Activate(scoreText, Score);
        }

        public void BringInChef(Graspable g)
        {
            continueButton.Show(false);
            chefEvaluation.SetActive(true);
        }

        public void Score(Graspable grabbable)
        {
            string result = "";
            int score = 0;
            int total = 0;
            foreach (var p in pourables)
            {
                bool correct = saladRecipe.IsCorrect(p.name, p.ActualAmount);
                if (correct)
                    score++;
                total++;
                result += string.Format("\n{0}: {1}. ", p.name, correct ? "correct" : "wrong");
            }

            foreach (var kv in addedItems)
            {
                bool correct = true; // made false if there are prep steps and they're wrong
                if (prepSteps.ContainsKey(kv.Key))
                {
                    var arr = prepSteps[kv.Key].ToArray();
                    if (arr.Length > 0)
                    {
                        if (!saladRecipe.PreparationCorrect(kv.Key, arr))
                            correct = false;
                    }
                }
                else if (kv.Value != 0)
                    correct = false;

                correct = correct && saladRecipe.IsCorrect(kv.Key, kv.Value);
                if (correct)
                    score++;
                total++;
                result += string.Format("\n{0}: {1}", kv.Key, correct ? "correct" : "wrong");
            }

            Debug.Log(result);

            float unitVal = Utils.UnitValueChallengeScore(score, total);

            int activityScore = Utils.ConvertToActivityScore(unitVal);
            Utils.RegisterActivityScore(activityScore);

            infoPanel.SetText(string.Format("Your score is {0} out of {1}", activityScore, ActivitySettings.pointsPerChallenge));
            if (unitVal > .85f)
                chefExpression.Happy();
            else if (unitVal > .45f)
                chefExpression.Skeptical();
            else
                chefExpression.Angry();

            infoPanel.ShowFor(ActivitySettings.Asset.infoDisplayTime, () =>
            {
                ActivityManager.Instance.FadeOutThen(() => { ActivityManager.Instance.Next(); });
            });
        }

        private void DetailedReport()
        {
            string result = "Detailed report: ";

            foreach (var p in pourables)
                result += string.Format("{0}: {1}. ", p.name, saladRecipe.GetResult(p.name, p.ActualAmount));

            foreach (var kv in addedItems)
            {
                string prepStuff = "";
                if (prepSteps.ContainsKey(kv.Key))
                {
                    prepStuff += "Preparation of ";

                    var arr = prepSteps[kv.Key].ToArray();
                    if (arr.Length > 0)
                    {
                        prepStuff += string.Join(", ", arr);
                        if (saladRecipe.PreparationCorrect(kv.Key, arr))
                            prepStuff += " is correct. ";
                        else
                            prepStuff += " is NOT correct. ";
                    }
                }
                result += string.Format("{0}: {1}. {2}", kv.Key, saladRecipe.GetResult(kv.Key, kv.Value), prepStuff);
            }
            Debug.Log(result);

        }
        private void OnEnterZone(Placeable p, Receivable r)
        {
            Debug.Log("OnEnterZone");

            Pourable pourable = p as Pourable;
            if (pourable)
            {
                dialHandler.SetMetric(0, pourable.maxUnits, pourable.metric);
                dialHandler.Set(pourable.PourAccumulator);
                dialHandler.Show(true);
            }
        }

        private void OnExitZone(Placeable p, Receivable r)
        {
            Debug.Log("OnExitZone");

            dialHandler.Show(false);
        }

        private void OnPlaceableDrop(Placeable placeable, Receivable r, bool inRange)
        {            
            if (inRange && !activeMenu)
            {
                int accum = 1;
                if (addedItems.ContainsKey(placeable.name))
                    accum += addedItems[placeable.name];
                addedItems[placeable.name] = accum;
                if (ingredientDestination)
                    placeable.MoveToPosition(ingredientDestination.position);
                else
                    placeable.MoveToReceiverPosition();

                // Only getting single prep step for all eggs, as it seems
                // redundant to allow seperate preparation for each one
                if (placeable.name == "Egg" && !prepSteps.ContainsKey("Egg"))
                {
                    eggPrep.Show();
                    activeMenu = true;
                    droppedEgg = true;
                }
                else if (placeable.name == "Garlic" && !prepSteps.ContainsKey("Garlic"))
                {
                    garlicPrep.Show();
                    activeMenu = true;
                    droppedGarlic = true;
                }
                else if (placeable.name == "Lettuce leaf" && !prepSteps.ContainsKey("Lettuce leaf"))
                {
                    lettucePrep.Show();
                    activeMenu = true;
                    droppedLettuce = true;
                }

            }
            else
                placeable.ReturnToInitialPosition();
        }

        private void OnPourDrop(Placeable placeable, Receivable r, bool inRange)
        {
            placeable.ReturnToInitialPosition();
        }

        private void OnPouring(Pourable pourable, float coeff)
        {
            if (dialHandler != null)
                dialHandler.Set(coeff);
        }

        private void OnFinishedPouring(Pourable pourable, float amt)
        {
            Debug.LogFormat("{0} poured {1} {2}", pourable.name, amt * pourable.maxUnits, pourable.metric);

            if (pourable.metric == "cup")
                pouredLiquid = true;
            else if (pourable.metric == "tsp")
                pouredGranular = true;
        }

        private void OnSliceableDrop(Placeable sliceable, Receivable r, bool inRange)
        {
            if (inRange && !activeMenu)
            {
                droppedBaguette = true;
                HandleProcessingOptions(sliceable as Sliceable);
            }
            else
                sliceable.ReturnToInitialPosition();
        }

        private void HandleProcessingOptions(Sliceable sliceable)
        {
            controlKnob.callOnUpdate = KnobControl;
            currrentSliceTarget = sliceable;
            controlKnob.Show(true);
            sliceable.PresentForSlicing();
            if (dropInfo)
                dropInfo.Show(true);
            controlKnob.clickUpAction = KnobReleased;
            addSlicesButton.Show(true);
            activeMenu = true;
        }

        /// <summary>
        /// This means we've finished the slicing
        /// session. UPDATE: noit doesn't
        /// </summary>
        /// <param name="g"></param>
        private void KnobReleased(Graspable g)
        {
            if (dropInfo)
                dropInfo.SetText("Add " + currrentSliceTarget.SliceCount + " slices");
        }

        private void OnTurningStoveKnob(float unitVal)
        {
            Debug.Log(unitVal);
        }

        private void KnobControl(float val)
        {
            if (currrentSliceTarget)
            {
                currrentSliceTarget.MarkSlices(Mathf.RoundToInt(val * currrentSliceTarget.maxSlices));
                if (dropInfo)
                    dropInfo.SetText(currrentSliceTarget.SliceCount.ToString());
            }
        }

        /// <summary>
        /// Hit Add button, so finished slicing
        /// </summary>
        /// <param name="g"></param>
        public void AddSlices(Graspable g)
        {
            controlKnob.callOnUpdate = null;
            controlKnob.clickUpAction = null;

            if (currrentSliceTarget)
            {
                addedItems[currrentSliceTarget.name] = currrentSliceTarget.SliceCount;

                if (dropInfo)
                {
                    dropInfo.SetText("Added " + currrentSliceTarget.SliceCount + " slices");
                    dropInfo.ShowFor(1f);
                    LeanTween.delayedCall(1f, () => 
                    {
                        baguetteSlicesPrep.Show();
                        activeMenu = true;
                    });
                }

                currrentSliceTarget.Show(false);
                currrentSliceTarget = null;
            }
            else if (dropInfo)
                dropInfo.Show(false);

            controlKnob.Show(false);
            addSlicesButton.Show(false);
            activeMenu = false;
        }

        public void SelectChoppedEgg()
        {
            Debug.Log("Chopped egg");

            eggPrep.Show(false);
            activeMenu = false;
            AddPrepStep("Egg", "Boiled and chopped");
        }

        public void SelectScrambledEgg()
        {
            Debug.Log("Scrambled egg");

            eggPrep.Show(false);
            activeMenu = false;
            AddPrepStep("Egg", "Scrambled");
        }

        public void SelectRawEgg()
        {
            Debug.Log("Raw cracked egg");

            eggPrep.Show(false);
            activeMenu = false;
            AddPrepStep("Egg", "Raw");
        }

        public void SelectButteredBaguette()
        {
            baguetteSlicesPrep.Show(false);
            AddPrepStep("Baguette", "Buttered");
            grilledBaguetteSlicesPrep.Show();
        }

        public void SelectSaltedBaguette()
        {
            baguetteSlicesPrep.Show(false);
            AddPrepStep("Baguette", "Salted");
            grilledBaguetteSlicesPrep.Show();
        }

        public void SelectOliveOilBrushedBaguette()
        {
            baguetteSlicesPrep.Show(false);
            AddPrepStep("Baguette", "Oiled");
            grilledBaguetteSlicesPrep.Show();
        }

        public void SelectChoppedBaguette()
        {
            grilledBaguetteSlicesPrep.Show(false);
            activeMenu = false;
            AddPrepStep("Baguette", "Chopped");
        }

        public void SelectJuliennedBaguette()
        {
            grilledBaguetteSlicesPrep.Show(false);
            activeMenu = false;
            AddPrepStep("Baguette", "Julienned");
        }

        public void SelectCrumbedBaguette()
        {
            grilledBaguetteSlicesPrep.Show(false);
            AddPrepStep("Baguette", "Crumbed");
            activeMenu = false;
        }

        public void SelectSlicedGarlic()
        {
            garlicPrep.Show(false);
            AddPrepStep("Garlic", "Sliced");
            activeMenu = false;
        }

        public void SelectCrushedGarlic()
        {
            garlicPrep.Show(false);
            AddPrepStep("Garlic", "Crushed");
            activeMenu = false;
        }

        public void SelectMincedGarlic()
        {
            garlicPrep.Show(false);
            AddPrepStep("Garlic", "Minced");
            activeMenu = false;
        }

        public void SelectTornLettuce()
        {
            lettucePrep.Show(false);
            AddPrepStep("Lettuce leaf", "Torn");
            activeMenu = false;
        }

        public void SelectChoppedLettuce()
        {
            lettucePrep.Show(false);
            AddPrepStep("Lettuce leaf", "Chopped");
            activeMenu = false;
        }

        public void SelectSlicedLettuce()
        {
            lettucePrep.Show(false);
            AddPrepStep("Lettuce leaf", "Sliced");
            activeMenu = false;
        }

        public void ChefArrived()
        {
            chefAnimatorController.SetTrigger("LookAtBench");
            chefExpression.Skeptical();
        }

        private void AddPrepStep(string itemName, string step)
        {
            if (!prepSteps.ContainsKey(itemName))
            {
                var prep = new List<string>
                {
                    step
                };
                prepSteps[itemName] = prep;
            }
            else
                prepSteps[itemName].Add(step);
        }

        private bool Done()
        {
            return !activeMenu && ((pouredLiquid || pouredGranular) && (droppedGarlic || droppedBaguette) && (droppedEgg || droppedLettuce));
        }

        private void RestorePositions()
        {
            foreach (var p in pourables)
                if (p.gameObject.activeInHierarchy)
                    p.ReturnToInitialPosition();
            foreach (var s in sliceables)
                if (s.gameObject.activeInHierarchy)
                    s.ReturnToInitialPosition();
            foreach (var a in placeables)
                if (a.gameObject.activeInHierarchy)
                    a.ReturnToInitialPosition();
        }
    }
}
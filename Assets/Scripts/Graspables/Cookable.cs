using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QS
{
    public class Cookable : Placeable
    {
        public Texture2D[] surfaceTextures;
        public int status;
        public Renderer modelRenderer;
        public GameObject feedbackCorrect, feedbackWrong;

        public enum CookedLevel { Raw, Undercooked, Cooked, Overcooked, Burnt };

        private LinkedList<string> prepSteps = new LinkedList<string>();
        private bool inPot; // So we know whether it's just raw or not
        private CookedLevel cookedLevel;
        private bool correctPrep;

        protected override void Awake()
        {
            base.Awake();

            if (surfaceTextures == null || surfaceTextures.Length == 0)
                Debug.LogError("Null or empty texture list");

            SetSurfaceTexture(0);
            Kinematic = false;
        }

        public void SetSurfaceTexture(int index)
        {
            if (surfaceTextures == null || surfaceTextures.Length <= index)
                Debug.LogError("Texture list too small");
            else
            {
                modelRenderer.sharedMaterial.mainTexture = surfaceTextures[index];
            }
        }

        /// <summary>
        /// This one's more direct but will only work if
        /// the assigned texture names match the range
        /// available to the caller
        /// </summary>
        /// <param name="texName"></param>
        public void SetSurfaceTexture(string texName)
        {
            Texture2D tex = surfaceTextures.FirstOrDefault(t => t.name == texName);
            if (tex)
            {
                modelRenderer.sharedMaterial.mainTexture = tex;
            }
            else
                Debug.LogWarning("Cookable texture not found: " + texName);
        }

        public bool AddPrepStep(string step)
        {
            //if (prepSteps.Count == 0 || prepSteps.Last.Value != step) // Don't care if they double-dip UPDATE: no doubling
            if (!prepSteps.Contains(step))
            {
                prepSteps.AddLast(step);
                return true;
            }
            return false;
        }

        public bool InPot { get; set; }

        public LinkedList<string> PrepSteps { get { return prepSteps; } }

        public bool CookedCorrectly()
        {
            return (cookedLevel == CookedLevel.Cooked);
        }

        public bool PreppedCorrectly()
        {
            return correctPrep;
        }

        public string GetCookedness()
        {
            if (cookedLevel == CookedLevel.Cooked)
                return "Nicely cooked";
            else
                return cookedLevel.ToString();
        }

        public string CorrectPrep
        {
            get { return correctPrep ? "correct" : "incorrect"; }
        }

        public void SetCookedStats(CookedLevel level, bool correctIngredients)
        {
            cookedLevel = level;
            correctPrep = correctIngredients;

            switch (cookedLevel)
            {
                case CookedLevel.Burnt:
                    SetSurfaceTexture(correctIngredients ? "burnt" : "wrongPrepBurnt");
                    break;
                case CookedLevel.Overcooked:
                    SetSurfaceTexture(correctIngredients ? "overcooked" : "wrongPrepOvercooked");
                    break;
                case CookedLevel.Cooked:
                    SetSurfaceTexture(correctIngredients ? "cooked" : "wrongPrepCooked");
                    break;
                case CookedLevel.Undercooked:
                    SetSurfaceTexture(correctIngredients ? "undercooked" : "wrongPrepUndercooked");
                    break;
            }

            // Set overall correctness for both ingredients and cook time
            SetAllCorrect(correctIngredients && level == CookedLevel.Cooked);
        }

        private void SetAllCorrect(bool allCorrect)
        {
            feedbackCorrect.SetActive(allCorrect);
            feedbackWrong.SetActive(!allCorrect);
        }
    }
}
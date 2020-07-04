using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    public class ResultAndScore
    {        
        public string result;
        public int score;

        public ResultAndScore(string r, int s)
        {
            result = r;
            score = s;
        }
    }

    /// <summary>
    /// Disrete unit a quantity range
    /// </summary>
    [System.Serializable]
    public class RangeItem
    {
        public float upTo;
        public string description;
        public int score;
        public bool correct;
    }

    /// <summary>
    /// A group of ranges for quantifying
    /// things like amounts to add
    /// </summary>
    [System.Serializable]
    public class Range
    {
        public RangeItem[] rangeItems;
        public string overshootRemark;
        public int overshootScore;

        public string GetDescription(float val)
        {
            for (int i = 0; i < rangeItems.Length; i++)
            {
                if (val < rangeItems[i].upTo)
                    return rangeItems[i].description;
            }

            return overshootRemark;
        }

        public int GetScore(float val)
        {
            for (int i = 0; i < rangeItems.Length; i++)
            {
                if (val < rangeItems[i].upTo)
                    return rangeItems[i].score;
            }

            return overshootScore;
        }

        public bool IsCorrect(float val)
        {
            for (int i = 0; i < rangeItems.Length; i++)
            {
                if (val < rangeItems[i].upTo)
                    return rangeItems[i].correct;
            }

            return false;
        }

        public int GetScoreForCorrect()
        {
            for (int i = 0; i < rangeItems.Length; i++)
            {
                if (rangeItems[i].correct)
                    return rangeItems[i].score;
            }
            return 0;
        }
    }

    [System.Serializable]
    public class RecipeItem
    {
        public string itemName;
        public string metric; // Measuring, e.e. tsp
        public Range range; // The boundary of the correct amount
        public string[] prepSteps; // The array contains only the correct item from each sequence
        public int correctPrepScore; // The value of each preparation sequence that's correct
    }

    [CreateAssetMenu()]
    public class Recipe : ScriptableObject
    {
        public string title;
        public RecipeItem[] recipeItems;        
        public int scoreMultiplier; // How important is the ratio of correct to unitVal calculated from MaxCorrect

        /// <summary>
        /// Get back a description of the
        /// success of the item's quantity
        /// </summary>
        /// <param name="item"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public string GetResult(string item, float val)
        {
            foreach (var i in recipeItems)
            {
                if (i.itemName.Equals(item, StringComparison.OrdinalIgnoreCase))            
                    return i.range.GetDescription(val);                
            }

            string result = string.Format("Item {0} not found.", item);
            Debug.LogError(result);
            return result;
        }

        public bool IsCorrect(string item, float val)
        {
            foreach (var i in recipeItems)
            {
                if (i.itemName.Equals(item, StringComparison.OrdinalIgnoreCase))
                    return i.range.IsCorrect(val);
            }
            return false;
        }

        public ResultAndScore GetResultAndScore(string item, float val)
        {
            foreach (var i in recipeItems)
            {
                if (i.itemName.Equals(item, StringComparison.OrdinalIgnoreCase))
                    return new ResultAndScore(
                        i.range.GetDescription(val),
                        i.range.GetScore(val));                    
            }

            string result = string.Format("Item {0} not found.", item);
            Debug.LogError(result);
            return new ResultAndScore(result, 0);
        }

        public int GetWeightedScore(int total)
        {
            float max = (float)GetMaxScore();
            float t = (float)total;
            return (int)(t / max * scoreMultiplier);
        }

        public int GetMaxScore()
        {
            int accum = 0;
            foreach (var i in recipeItems)
            {
                accum += i.range.GetScoreForCorrect();
                // PrepSteps is a list of sequences. This is the 
                // score for a correct answer for any sequence
                accum += i.prepSteps.Length * i.correctPrepScore;            
            }
            return accum;
        }

        /// <summary>
        /// Get back a boolean for the 
        /// success of the linear selection 
        /// of preparation steps
        /// </summary>
        /// <param name="item"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public bool PreparationCorrect(string item, string[] steps)
        {
            foreach (var r in recipeItems)
            {
                if (r.itemName == item)
                {
                    for (int i = 0; i < r.prepSteps.Length; i++)
                    {
                        if (i >= steps.Length || r.prepSteps[i] != steps[i])
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
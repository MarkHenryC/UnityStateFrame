using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    public class Morph : MonoBehaviour 
	{
        [Serializable] public class StringToBlendValue : SerializableDictionary<string, BlendValue[]> { }
        [Serializable] public class BlendValue { public int index; public float weight; }

        public SkinnedMeshRenderer smr;
        public StringToBlendValue expressionMap = new StringToBlendValue();
        public int blendshapeCount;

        private void OnEnable()
        {
            CountBlendShapes();    
        }

        private void OnValidate()
        {
            CountBlendShapes();
        }

        public void CountBlendShapes() 
		{
            blendshapeCount = smr.sharedMesh.blendShapeCount;
		}

        public void CreateExpression(string name)
        {
            List<BlendValue> blendVals = new List<BlendValue>();

            for (int i = 0; i < blendshapeCount; i++)
            {
                float w = smr.GetBlendShapeWeight(i);
                if (w != 0f)
                    blendVals.Add(new BlendValue { index = i, weight = w });
            }

            expressionMap[name] = blendVals.ToArray();
        }

        public void SetExpression(string name, bool resetShapes = true, float morphTime = .25f, float holdTime = 1f)
        {
            if (resetShapes)
                ResetShapes();

            MorphExpressionAndReturn(name, morphTime, holdTime);
        }

        public void SetExpressionImmediate(string name, bool resetShapes = true)
        {
            if (resetShapes)
                ResetShapes();

            if (expressionMap.ContainsKey(name))
            {
                BlendValue[] package = expressionMap[name];
                for (int i = 0; i < package.Length; i++)
                    smr.SetBlendShapeWeight(package[i].index, package[i].weight);                
            }
        }

        public void ResetShapes()
        {
            for (int i = 0; i < blendshapeCount; i++)
                smr.SetBlendShapeWeight(i, 0f);
        }

        public void MorphExpression(string name, float morphTime)
        {
            if (expressionMap.ContainsKey(name))
            {
                BlendValue[] package = expressionMap[name];
                for (int i = 0; i < package.Length; i++)
                    TweenSkin(smr, package[i].index, package[i].weight, morphTime);
            }
        }

        private void TweenSkin(SkinnedMeshRenderer smr, int index, float targetValue = 0, float timeDelta = 1f)
        {
            LeanTween.value(gameObject, (float f) =>
            {
                smr.SetBlendShapeWeight(index, f);
            },
            smr.GetBlendShapeWeight(index), targetValue, timeDelta);
        }

        public void MorphExpressionAndReturn(string name, float morphTime = 1f, float holdTime = 2f)
        {
            if (expressionMap.ContainsKey(name))
            {
                BlendValue[] package = expressionMap[name];
                for (int i = 0; i < package.Length; i++)
                    TweenAndReturnSkin(smr, package[i].index, package[i].weight, morphTime, holdTime);
            }
        }

        private void TweenAndReturnSkin(SkinnedMeshRenderer smr, int index, float targetValue, float timeDelta, float holdTime)
        {
            LeanTween.value(gameObject, (float f) =>
            {
                smr.SetBlendShapeWeight(index, f);
            },
            smr.GetBlendShapeWeight(index), targetValue, timeDelta)
            .setOnComplete(() =>
            {
                LeanTween.delayedCall(holdTime, () =>
                {
                    LeanTween.value(gameObject, (float f) =>
                    {
                        smr.SetBlendShapeWeight(index, f);
                    },
                    smr.GetBlendShapeWeight(index), 0f, timeDelta);
                });
            });
        }
    }
}
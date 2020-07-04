using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    [Serializable] public class StringToBlendPackage : SerializableDictionary<string, BlendPackage> { }
    [Serializable] public class BlendValue { public int index; public float weight; }
    [Serializable] public class BlendPackage { public BlendValue[] bodyValues; public BlendValue[] eyelashValues; }

    public class FacialExpressions : MonoBehaviour 
	{
        // These are mapped to the Adobe Fuse blend-shapes
        public SkinnedMeshRenderer body, eyelash;
        public StringToBlendPackage expressionMap = new StringToBlendPackage();
        public int bodyBlendshapeCount, eyelashBlendshapeCount;

        private BlendValue[] bodyArray, eyelashArray;        

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
            bodyBlendshapeCount = body.sharedMesh.blendShapeCount;
            eyelashBlendshapeCount = eyelash ? eyelash.sharedMesh.blendShapeCount : 0;
		}

        public void CreateExpression(string name)
        {
            List<BlendValue> bodyVals = new List<BlendValue>();
            List<BlendValue> eyelashVals = new List<BlendValue>();

            for (int i = 0; i < bodyBlendshapeCount; i++)
            {
                float w = body.GetBlendShapeWeight(i);
                if (w != 0f)
                    bodyVals.Add(new BlendValue { index = i, weight = w });
            }

            for (int i = 0; i < eyelashBlendshapeCount; i++)
            {
                float w = eyelash.GetBlendShapeWeight(i);
                if (w != 0f)
                    eyelashVals.Add(new BlendValue { index = i, weight = w });
            }

            BlendPackage package = new BlendPackage
            {
                bodyValues = bodyVals.ToArray(),
                eyelashValues = eyelashVals.ToArray()
            };

            expressionMap[name] = package;
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
                BlendPackage package = expressionMap[name];
                for (int i = 0; i < package.bodyValues.Length; i++)
                    body.SetBlendShapeWeight(package.bodyValues[i].index, package.bodyValues[i].weight);                

                for (int i = 0; i < package.eyelashValues.Length; i++)
                    eyelash.SetBlendShapeWeight(package.bodyValues[i].index, package.bodyValues[i].weight);
            }
        }

        public void ResetShapes()
        {
            for (int i = 0; i < bodyBlendshapeCount; i++)
                body.SetBlendShapeWeight(i, 0f);
            for (int i = 0; i < eyelashBlendshapeCount; i++)
                eyelash.SetBlendShapeWeight(i, 0f);

        }

        public void MorphExpression(string name, float morphTime)
        {
            if (expressionMap.ContainsKey(name))
            {
                BlendPackage package = expressionMap[name];
                for (int i = 0; i < package.bodyValues.Length; i++)
                    TweenSkin(body, package.bodyValues[i].index, package.bodyValues[i].weight, morphTime);

                for (int i = 0; i < package.eyelashValues.Length; i++)
                    TweenSkin(eyelash, package.bodyValues[i].index, package.bodyValues[i].weight, morphTime);
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
                BlendPackage package = expressionMap[name];
                for (int i = 0; i < package.bodyValues.Length; i++)
                    TweenAndReturnSkin(body, package.bodyValues[i].index, package.bodyValues[i].weight, morphTime, holdTime);

                for (int i = 0; i < package.eyelashValues.Length; i++)
                    TweenAndReturnSkin(eyelash, package.bodyValues[i].index, package.bodyValues[i].weight, morphTime, holdTime);
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
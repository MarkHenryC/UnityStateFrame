using System;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    [Serializable] public class BlendPair { public int index; public float weight; }
    [Serializable] public class BlendValueGroup { public BlendPair[] blendValues; }
    [Serializable] public class BlendMap : SerializableDictionary<string, BlendValueGroup> { }

    public class BlendShapeManager : MonoBehaviour
    {
        public SkinnedMeshRenderer[] skinnedMeshRenderers;
        public BlendMap blendMap = new BlendMap();

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
            if (skinnedMeshRenderers.Length > 0)
                blendshapeCount = skinnedMeshRenderers[0].sharedMesh.blendShapeCount;
        }

        public void CreateExpression(string name)
        {
            List<BlendPair> blendPairs = new List<BlendPair>();

            for (int i = 0; i < blendshapeCount; i++)
            {
                float w = skinnedMeshRenderers[0].GetBlendShapeWeight(i);
                if (w != 0f)
                    blendPairs.Add(new BlendPair { index = i, weight = w });
            }

            BlendValueGroup group = new BlendValueGroup
            {
                blendValues = blendPairs.ToArray(),
            };

            blendMap[name] = group;
        }

        public void ResetShapes()
        {
            for (int i = 0; i < blendshapeCount; i++)
            {
                skinnedMeshRenderers[0].SetBlendShapeWeight(i, 0f);
                string name = skinnedMeshRenderers[0].sharedMesh.GetBlendShapeName(i);
                for (int j = 0; j < skinnedMeshRenderers.Length; j++)
                {
                    int index = skinnedMeshRenderers[j].sharedMesh.GetBlendShapeIndex(name);
                    if (index >= 0)
                        skinnedMeshRenderers[j].SetBlendShapeWeight(index, 0f);
                }
            }
        }

        /// <summary>
        /// Runtime morph
        /// </summary>
        /// <param name="name"></param>
        /// <param name="morphTime"></param>
        /// <param name="holdTime"></param>
        public void SetExpression(string name, float morphTime = 1f, float holdTime = 2f)
        {
            if (blendMap.ContainsKey(name))
            {
                BlendValueGroup group = blendMap[name];
                for (int i = 0; i < group.blendValues.Length; i++)
                {
                    float weight = group.blendValues[i].weight;
                    int primaryIndex = group.blendValues[i].index;

                    TweenMorph(skinnedMeshRenderers[0], primaryIndex, weight, morphTime, holdTime);
                    string blendName = skinnedMeshRenderers[0].sharedMesh.GetBlendShapeName(primaryIndex);

                    for (int j = 1; j < skinnedMeshRenderers.Length; j++)
                    {
                        int correspondingIndex = skinnedMeshRenderers[j].sharedMesh.GetBlendShapeIndex(blendName);
                        if (correspondingIndex >= 0)
                            TweenMorph(skinnedMeshRenderers[j], correspondingIndex, weight, morphTime, holdTime);
                    }
                }
            }
        }

        /// <summary>
        /// For editor preview
        /// </summary>
        /// <param name="name"></param>
        /// <param name="morphTime"></param>
        /// <param name="holdTime"></param>
        public void SetExpressionImmediate(string name, float morphTime = 1f)
        {
            if (blendMap.ContainsKey(name))
            {
                BlendValueGroup group = blendMap[name];
                for (int i = 0; i < group.blendValues.Length; i++)
                {
                    float weight = group.blendValues[i].weight;
                    int primaryIndex = group.blendValues[i].index;

                    SetSkin(skinnedMeshRenderers[0], primaryIndex, weight);
                    string blendName = skinnedMeshRenderers[0].sharedMesh.GetBlendShapeName(primaryIndex);

                    for (int j = 1; j < skinnedMeshRenderers.Length; j++)
                    {
                        int correspondingIndex = skinnedMeshRenderers[j].sharedMesh.GetBlendShapeIndex(blendName);
                        if (correspondingIndex >= 0)
                            SetSkin(skinnedMeshRenderers[j], correspondingIndex, weight);
                    }
                }
            }
        }

        private void SetSkin(SkinnedMeshRenderer smr, int index, float targetValue = 0)
        {
            smr.SetBlendShapeWeight(index, targetValue);
        }

        private void TweenMorph(SkinnedMeshRenderer smr, int index, float targetValue = 0, float timeDelta = 1f, float holdTime = 2f)
        {
            LeanTween.value(gameObject, (float f) =>
            {
                smr.SetBlendShapeWeight(index, f);
            },
            smr.GetBlendShapeWeight(index), targetValue, timeDelta)
            .setOnComplete(() =>
            {
                if (holdTime != 0f)
                {
                    LeanTween.delayedCall(holdTime, () =>
                    {
                        LeanTween.value(gameObject, (float f) =>
                        {
                            smr.SetBlendShapeWeight(index, f);
                        },
                        smr.GetBlendShapeWeight(index), 0f, timeDelta);

                    });
                }
            });
        }
    }
}
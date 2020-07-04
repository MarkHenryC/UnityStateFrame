using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    /// <summary>
    /// This is tailored for the chef model supplied by
    /// Freelancer artist. Blend shapes were not mapped
    /// to animations so I created this class. Something
    /// more generic, or maybe animation-mapped
    /// blendshapes would be better next time.
    /// </summary>
	public class ExpressionHandler : MonoBehaviour 
	{
        public GameObject eyebrows, face;
        public float expressionChangeTime = 1f;

        private SkinnedMeshRenderer eyebrowSkin, faceSkin;
        private const int
            EyebrowAngry = 0, EyebrowSkeptical = 1, EyebrowHappy = 2,
            EyelidsClosed = 0, EyelidLeftClosed = 1, EyelidRightClosed = 2,
            MouthSmile = 3, MouthSkeptical = 4, MouthAngry = 5;
        private const float 
            OffPos = 0, EyelidSkepticalPos = 65f, EyelidNeutralPos = 25f,
            MouthNeutralPos = 50f, OnPos = 100f;

        void Awake () 
		{
            Init();
        }
		
        public void Init()
        {
            eyebrowSkin = eyebrows.GetComponent<SkinnedMeshRenderer>();
            faceSkin = face.GetComponent<SkinnedMeshRenderer>();
        }

        public void ResetSkins()
        {
            eyebrowSkin.SetBlendShapeWeight(EyebrowAngry, OffPos); // frown
            eyebrowSkin.SetBlendShapeWeight(EyebrowSkeptical, OffPos); // skeptical
            eyebrowSkin.SetBlendShapeWeight(EyebrowHappy, OffPos); // raised/happy

            faceSkin.SetBlendShapeWeight(EyelidsClosed, OffPos); // eyelids closed
            faceSkin.SetBlendShapeWeight(EyelidLeftClosed, OffPos); // left eyelid closed
            faceSkin.SetBlendShapeWeight(EyelidRightClosed, OffPos); // right eyelid closed

            faceSkin.SetBlendShapeWeight(MouthSmile, OffPos); // smile
            faceSkin.SetBlendShapeWeight(MouthSkeptical, OffPos); // doubtful mouth
            faceSkin.SetBlendShapeWeight(MouthAngry, OffPos); // angry mouth
        }

        public void Skeptical()
        {
            TweenSkin(eyebrowSkin, EyebrowAngry, OffPos);
            TweenSkin(eyebrowSkin, EyebrowSkeptical);
            TweenSkin(eyebrowSkin, EyebrowHappy, OffPos);

            TweenSkin(faceSkin, EyelidsClosed, EyelidSkepticalPos);
            TweenSkin(faceSkin, EyelidLeftClosed, OffPos);
            TweenSkin(faceSkin, EyelidRightClosed, OffPos);

            TweenSkin(faceSkin, MouthSmile, OffPos);
            TweenSkin(faceSkin, MouthSkeptical);
            TweenSkin(faceSkin, MouthAngry, OffPos);

        }

        public void Angry()
        {
            TweenSkin(eyebrowSkin, EyebrowAngry);
            TweenSkin(eyebrowSkin, EyebrowSkeptical, OffPos);
            TweenSkin(eyebrowSkin, EyebrowHappy, OffPos);

            TweenSkin(faceSkin, EyelidsClosed, OffPos);
            TweenSkin(faceSkin, EyelidLeftClosed, OffPos);
            TweenSkin(faceSkin, EyelidRightClosed, OffPos);

            TweenSkin(faceSkin, MouthSmile, OffPos);
            TweenSkin(faceSkin, MouthSkeptical, OffPos);
            TweenSkin(faceSkin, MouthAngry);
        }

        public void Happy()
        {
            TweenSkin(eyebrowSkin, EyebrowAngry, OffPos);
            TweenSkin(eyebrowSkin, EyebrowSkeptical, OffPos);
            TweenSkin(eyebrowSkin, EyebrowHappy);

            TweenSkin(faceSkin, EyelidsClosed, OffPos);
            TweenSkin(faceSkin, EyelidLeftClosed, OffPos);
            TweenSkin(faceSkin, EyelidRightClosed, OffPos);

            TweenSkin(faceSkin, MouthSmile);
            TweenSkin(faceSkin, MouthSkeptical, OffPos);
            TweenSkin(faceSkin, MouthAngry, OffPos);
        }

        public void Neutral()
        {
            TweenSkin(eyebrowSkin, EyebrowAngry, OffPos); // angry
            TweenSkin(eyebrowSkin, EyebrowSkeptical, OffPos); // skeptical
            TweenSkin(eyebrowSkin, EyebrowHappy, OffPos); // happy

            TweenSkin(faceSkin, EyelidsClosed, EyelidNeutralPos); // both eyelids closed
            TweenSkin(faceSkin, EyelidLeftClosed, OffPos); // left eyelid closed
            TweenSkin(faceSkin, EyelidRightClosed, OffPos); // right eyelid closed

            TweenSkin(faceSkin, MouthSmile, OffPos); // mouth happy
            TweenSkin(faceSkin, MouthSkeptical, OffPos); // mouth skeptical
            TweenSkin(faceSkin, MouthAngry, MouthNeutralPos); // mouth angry
        }

        /// <summary>
        /// Same as reset except tweened
        /// </summary>
        public void Default()
        {
            TweenSkin(eyebrowSkin, EyebrowAngry, OffPos); // angry
            TweenSkin(eyebrowSkin, EyebrowSkeptical, OffPos); // skeptical
            TweenSkin(eyebrowSkin, EyebrowHappy, OffPos); // happy

            TweenSkin(faceSkin, EyelidsClosed, OffPos); // both eyelids closed
            TweenSkin(faceSkin, EyelidLeftClosed, OffPos); // left eyelid closed
            TweenSkin(faceSkin, EyelidRightClosed, OffPos); // right eyelid closed

            TweenSkin(faceSkin, MouthSmile, OffPos); // mouth happy
            TweenSkin(faceSkin, MouthSkeptical, OffPos); // mouth skeptical
            TweenSkin(faceSkin, MouthAngry, OffPos); // mouth angry
        }

        private void TweenSkin(SkinnedMeshRenderer smr, int index, float targetValue = OnPos)
        {
            LeanTween.value(gameObject, (float f) =>
            { smr.SetBlendShapeWeight(index, f); },
                smr.GetBlendShapeWeight(index), targetValue, expressionChangeTime);
        }
    }
}
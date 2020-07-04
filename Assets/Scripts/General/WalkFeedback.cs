using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class WalkFeedback : MonoBehaviour
    {
        public AudioClip leftFoot, rightFoot, skid, roll;

        public void LeftFoot()
        {
            LeanAudio.playClipAt(leftFoot, transform.position).spatialBlend = .5f;
        }

        public void RightFoot()
        {
            LeanAudio.playClipAt(rightFoot, transform.position).spatialBlend = .5f;
        }

        public void Skid()
        {
            LeanAudio.playClipAt(skid, transform.position).spatialBlend = .5f;
        }

        public void Roll()
        {
            LeanAudio.playClipAt(roll, transform.position).spatialBlend = .5f;
        }
    }
}
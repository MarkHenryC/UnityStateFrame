using System;
using UnityEngine;
using UnityEngine.Events;

namespace QS
{
    /// <summary>
    /// Simple way to move an along a path and
    /// optionally call a UnityEvent and/or 
    /// a system action upon arrival.
    /// </summary>
	public class SimplePathAnimator : MonoBehaviour
    {
        public Transform[] waypoints;
        public float time;
        public Action OnComplete;
        public bool playOnEnable;
        public UnityEvent ueOnComplete;
        public AudioClip moveSound;
        public bool cycle;

        private Vector3[] animPath;
        private float[] animationPoints; // converted to relative time
        private AudioSource moveSoundPlayer;

        private void Awake()
        {
            moveSoundPlayer = GetComponent<AudioSource>();
            if (waypoints != null && waypoints.Length > 0)
                animPath = Utils.SimpleXZSplinePath(transform, waypoints, cycle);
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Run();
        }

        public void Run()
        {
            if (animPath == null)
                return;

            Debug.Log("*** Run() called in SimplePathAnimator ***");

            if (cycle)
            {
                LeanTween.moveSpline(gameObject, animPath, time)
                    .setOrientToPath(true)
                    .setLoopClamp();
            }
            else
            {
                LeanTween.moveSpline(gameObject, animPath, time)
                //.setEase(LeanTweenType.easeOutQuad) // No easing until I sync animation
                .setOrientToPath(true)
                .setOnComplete(() =>
                {
                    if (OnComplete != null)
                    {
                        Debug.Log("Calling OnComplete");
                        OnComplete();
                    }
                    OnMoveComplete();

                    if (ueOnComplete != null)
                    {
                        Debug.Log("Calling ueOnComplete");
                        ueOnComplete.Invoke();
                    }
                });
            }
        }

        private void OnMoveComplete()
        {
            if (moveSoundPlayer && moveSound)
                moveSoundPlayer.Stop();
        }
    }
}
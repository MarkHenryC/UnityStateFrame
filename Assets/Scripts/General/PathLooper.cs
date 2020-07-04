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
	public class PathLooper : MonoBehaviour
    {
        public Transform[] waypoints;
        public float time;
        public Action OnComplete;
        public bool playOnEnable;
        public UnityEvent ueOnComplete;
        public float[] unitValAnimationPoints;
        //public Animator animator;
        public Animation animationCycler;
        public string startAnimName, endAnimName;
        public float startAnimSpeed, endAnimSpeed;
        public WrapMode startWrapMode, endWrapMode;
        public float crossfadeTime;
        public float unitValToEnd;
        public AudioClip moveSound;
        public float secondsBeforeAnimStop = 5f;
        public PlayType playType = PlayType.Once;
        public int repeats = 0;
        public bool staticOrientation;

        public enum PlayType { Once, Repeat, PingPong, Cycle };

        private Vector3[] animPath;
        private float[] animationPoints; // converted to relative time
        private float counter;
        private bool crossedThreshold;
        private float timeToEnd;
        private AudioSource moveSoundPlayer;
        private Vector3 defaultPos;
        private Quaternion defaultRot;
        private LTDescr playingAnim;

        private void Awake()
        {
            moveSoundPlayer = GetComponent<AudioSource>();
            if (waypoints != null && waypoints.Length > 0)
            {
                bool cycle = (playType == PlayType.Cycle) ? true : false;
                animPath = Utils.SimpleXZSplinePath(transform, waypoints, cycle);
            }

            if (animPath != null && animationCycler && unitValAnimationPoints.Length > 0)
            {
                animationPoints = new float[unitValAnimationPoints.Length];
                for (int i = 0; i < unitValAnimationPoints.Length; i++)
                    animationPoints[i] = time * unitValAnimationPoints[i];
            }

            defaultPos = gameObject.transform.position;
            defaultRot = gameObject.transform.rotation;
        }

        private void OnEnable()
        {
            counter = 0f;
            timeToEnd = time * unitValToEnd;
            crossedThreshold = false;

            if (playOnEnable)
                Run();
        }

        private void Update()
        {
            if (animationCycler && !crossedThreshold)
            {
                counter += Time.deltaTime;
                if (counter >= timeToEnd)
                {
                    crossedThreshold = true;
                }
            }
        }

        public void Run()
        {
            if (playType == PlayType.Cycle)
            {
                playingAnim = LeanTween.moveSpline(gameObject, animPath, time)
                    .setOrientToPath(!staticOrientation)
                    .setLoopClamp();
            }
            else
            {
                playingAnim = LeanTween.moveSpline(gameObject, animPath, time)
                //.setEase(LeanTweenType.easeOutQuad) // No easing until I sync animation
                .setOrientToPath(!staticOrientation)
                .setOnComplete(() =>
                {
                    if (OnComplete != null)
                        OnComplete();
                    OnMoveComplete();
                    if (ueOnComplete != null)
                        ueOnComplete.Invoke();
                });

                if (playType == PlayType.Repeat)
                    playingAnim.setRepeat(repeats);
                else if (playType == PlayType.PingPong)
                    playingAnim.setLoopPingPong();
            }

            if (animationCycler)
            {
                animationCycler[startAnimName].speed = startAnimSpeed;
                animationCycler[startAnimName].wrapMode = startWrapMode;
                animationCycler.clip = animationCycler[startAnimName].clip;
                animationCycler.Play();
                if (moveSoundPlayer && moveSound)
                {
                    moveSoundPlayer.clip = moveSound;
                    moveSoundPlayer.Play();
                }
            }
        }

        public void Stop(bool reset = true)
        {
            if (reset)
            {
                gameObject.transform.position = defaultPos;
                gameObject.transform.rotation = defaultRot;
            }

            if (playingAnim != null)
                LeanTween.cancel(playingAnim.id);
                
        }

        private void OnMoveComplete()
        {
            if (animationCycler)
            {
                //animator.Play(endAnimName);
                animationCycler[endAnimName].speed = endAnimSpeed;
                animationCycler[endAnimName].wrapMode = endWrapMode;

                // Quick jump option:
                animationCycler.Stop();
                animationCycler.clip = animationCycler[endAnimName].clip;
                animationCycler.Play();

                // Crossfade option: (too slow, even at crossfaded time of 0.05 seconds - must be playing at least one cycle)
                //animationCycler.CrossFade(endAnimName, crossfadeTime);

                if (moveSoundPlayer && moveSound)
                    moveSoundPlayer.Stop();
                LeanTween.delayedCall(secondsBeforeAnimStop, () => { animationCycler.Stop(); });
            }
        }
    }
}
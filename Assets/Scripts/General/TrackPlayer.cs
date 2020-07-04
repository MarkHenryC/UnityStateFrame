using System.Collections.Generic;
using UnityEngine;
using DentedPixel;

namespace QuiteSensible
{
    /// <summary>
    /// Music player
    /// </summary>
    public class TrackPlayer : MonoBehaviour
    {
        public AudioClip clip;
        public float fadeLength = 5f;
        public bool oneShot = false;
        public AnimationCurve fadeout;
        [Range(0f, 1f)]
        public float volume = 0.5f;
        public bool playOnEnable, playOnDisable;

        private AudioSource source;
        private float countTime;
        private float fadeStartTime;
        private Queue<AudioClip> audioClipQueue;
        private bool paused;
        private System.Action onFadedOut;
        private bool stopping;

        private void Awake()
        {
            paused = false;
            source = GetComponent<AudioSource>();
            audioClipQueue = new Queue<AudioClip>();

            if (!oneShot)
                source.loop = true;
            else
                source.loop = false;
        }

        private void OnEnable()
        {
            if (playOnEnable && clip)
                Play();
        }

        private void OnDisable()
        {
            if (playOnDisable && clip)
                Play();
        }

        private void Start()
        {
        }

        public void Play(bool restart = false)
        {
            if (restart || (source == null || !source.isPlaying))
                Play(clip);
        }

        public void StopPlay()
        {
            Debug.Log("StopPlay: playing? " + source.isPlaying);
            source.Stop();
        }

        public void Play(AudioClip pClip, float delay = 0f)
        {
            clip = pClip;
            audioClipQueue.Clear(); // even if null clip

            if (clip)
            {
                fadeStartTime = clip.length - fadeLength;

                countTime = 0f;
                source.clip = clip;
                source.volume = volume;
                if (delay > 0f)
                    LeanTween.delayedCall(delay, () => { source.Play(); });
                else
                    source.Play();
            }
        }

        public void Stop(System.Action onFadeout = null)
        {
            if (onFadeout == null)
                source.Stop();
            else
            {
                onFadedOut = onFadeout;
                countTime = fadeStartTime;
                stopping = true;
            }
        }

        public void Pause(bool pause = true)
        {
            paused = pause;
        }

        public bool Playing
        {
            get { return source && source.isPlaying; }
        }

        public void Cue(AudioClip next)
        {
            audioClipQueue.Enqueue(next);
            Debug.Log(">>>>> Enqueueing " + next.name);
        }

        private void Update()
        {
            if (paused)
                return;

            if (source.isPlaying)
            {
                countTime += Time.deltaTime;

                if (countTime >= fadeStartTime && fadeLength > 0f)
                {
                    source.volume = volume * fadeout.Evaluate(1f - (clip.length - countTime) / fadeLength);
                    if (source.volume < 0.01f)
                    {
                        source.volume = volume; // reset to original
                        countTime = 0f;

                        if (stopping || oneShot)
                        {
                            Debug.Log("TrackPlayer: Play stopped");

                            // Purpose of this is to allow a graceful
                            // fade when exiting scene, rather than
                            // an abrupt halt. So the scene chance
                            // code is in the onFadedOut action.
                            source.Stop();
                            if (onFadedOut != null)
                                onFadedOut();
                            stopping = false;
                        }
                        else
                            Play();
                    }
                    else if (source.volume > volume)
                        source.volume = volume; // protection agains overflow
                }
            }
            else if (audioClipQueue != null && audioClipQueue.Count > 0)
            {
                AudioClip next = audioClipQueue.Dequeue();
                Debug.Log("<<<<< Dequeueing " + next.name);
                Play(next);
            }
        }
    }
}
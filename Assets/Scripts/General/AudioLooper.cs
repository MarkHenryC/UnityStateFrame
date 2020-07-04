using System.Collections;
using UnityEngine;

namespace QS
{
    /// <summary>
    /// Assumptions:
    /// 1. Loop by default, but can play through to finish
    /// 2. User has correctly set loop points
    /// 3. Sample is decompressed on load
    /// </summary>
    public class AudioLooper : MonoBehaviour
    {
        public int sampleRate = 44100;
        public float startLoopTime, endLoopTime;
        public float fadeoutSeconds = 1f;
        public AudioSource audioSource;
        public AudioClip audioClip;

        [Header("For UI testing only")]

        [Tooltip("Play audio on scene play")]
        public bool playOnStart;

        private bool isPlaying, isLooping;
        private int startLoopSamples, endLoopSamples;
        private float origVol;

        private void Awake()
        {
            isLooping = true;
            if (audioClip)
                SetClip(audioClip, startLoopTime, endLoopTime);
        }

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        private void Update()
        {
            if (isPlaying)
            {
                if (isLooping)
                {
                    if (audioSource.timeSamples >= endLoopSamples)
                        audioSource.timeSamples = startLoopSamples;
                }

                // Sync with actual audio. Need this for
                // a Playthrough()
                if (!audioSource.isPlaying)
                    isPlaying = false;
            }
        }

        public void SetClip(AudioClip clip, float startL = 0, float endL = 0)
        {
            startLoopSamples = (int)(sampleRate * startL);
            if (endL == 0) // loop entire clip
            {
                endLoopSamples = clip.samples;
                audioSource.loop = true; // Use built-in loop method
            }
            else
                endLoopSamples = (int)(sampleRate * endL);

            audioClip = clip;
            audioSource.clip = audioClip;
            origVol = audioSource.volume;
        }

        public void SetPitch(float coeff)
        {
            audioSource.pitch = coeff;
        }

        public void Play(bool loop = true)
        {
            isPlaying = true;
            isLooping = loop;
            audioSource.Play();
        }

        public void Stop()
        {
            isPlaying = false;
            audioSource.Stop();
        }

        public void SetMode(bool on)
        {
            if (on && !isPlaying)
                Play();
            else if (!on && isPlaying)
                Stop();
        }

        public void PlayThrough()
        {
            isLooping = false;
        }

        public void Sync(bool on, bool loop = true)
        {
            if (on && !isPlaying)
                Play(loop);
            else if (!on && isPlaying)
                Fadeout();
        }

        public void Fadeout()
        {
            StartCoroutine(DoFadeout(fadeoutSeconds));
        }

        public bool Playing => isPlaying;

        private IEnumerator DoFadeout(float duration = 1f)
        {
            float timeCounter = 0f;
            float t = 1f / duration;

            while (timeCounter <= 1f)
            {
                timeCounter += t * Time.deltaTime;
                audioSource.volume = 1f - timeCounter;

                yield return null;
            }

            Stop();
            audioSource.volume = origVol;
        }
    }

}
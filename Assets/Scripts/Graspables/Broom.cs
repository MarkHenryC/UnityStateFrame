using UnityEngine;

namespace QS
{
    public class Broom : Graspable
    {
        [Tooltip("The distance we use to measure swipe speed")]
        public float measureDistance;
        [Tooltip("The timeframe we should travel the squared distance")]
        public float timeForDistance;
        [Tooltip("Any associated animation")]
        public GameObject mopEffect;
        public AudioLooper audioLooper;
        public bool returnOnRelease;

        private Vector3 lastPos = Vector3.zero;
        private Vector3 initialPos = Vector3.zero;
        private Vector3 lastDirection = Vector3.zero;
        private float measureTime;

        private const float distanceSpeedForFx = 0.001f;

        public override void OnTriggerClickDown(VrEventInfo info)
        {
            base.OnTriggerClickDown(info);
            lastPos = transform.position;
            initialPos = lastPos;
        }

        public override void OnTriggerClickUp(VrEventInfo info)
        {
            base.OnTriggerClickUp(info);
            if (returnOnRelease)
                transform.position = initialPos; // TODO: animate this
            audioLooper.Stop();
            if (mopEffect)
                mopEffect.SetMode(false);
        }

        public override void MoveTo(Vector3 newPos)
        {
            base.MoveTo(newPos);

            HandleAudio(newPos);
        }

        private void HandleAudio(Vector3 newPos)
        {
            // A gentle bit of audio feedback for
            // swishing back and forth

            Vector3 moved = newPos - lastPos;
            float distance = moved.magnitude;

            bool createEffect = (distance > distanceSpeedForFx);

            if (mopEffect)
                mopEffect.SetMode(createEffect);

            float elapsed = Time.time - measureTime;
            if (elapsed >= timeForDistance)
            {
                if (distance >= measureDistance)
                {
                    if (!audioLooper.Playing)
                        audioLooper.Play();

                    // Temp stop if changing direction
                    if (Vector3.Dot(moved, lastDirection) < 0)
                    {
                        //Debug.Log("DIRECTION CHANGE");

                        if (audioLooper.Playing)
                            audioLooper.Stop();
                    }
                }
                else if (audioLooper.Playing)
                {
                    if (mopEffect)
                        mopEffect.SetMode(false);
                    audioLooper.Stop();
                }

                measureTime = Time.time;
                lastDirection = moved;
                lastPos = newPos;
            }
        }
    }
}
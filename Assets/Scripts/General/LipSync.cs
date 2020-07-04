using UnityEngine;

public abstract class LipSync : MonoBehaviour
{
    private AudioClip audioClip;

    public bool IsPlaying { get; }
    
    public void SetAudioClip(AudioClip clip)
    {
        audioClip = clip;
    }

    public virtual void Play()
    {

    }
}

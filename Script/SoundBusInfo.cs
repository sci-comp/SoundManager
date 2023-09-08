using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public enum SoundBUS
{
    Ambient,
    Environment,
    SFX,
    UI,
    Voice
}

[CreateAssetMenu(fileName = "SoundBusInfo", menuName = "Audio/SoundBusInfo")]
public class SoundBusInfo : ScriptableObject
{
    public SoundBUS soundBus;
    public AudioMixer audioMixer;
    public float individualVolume = 1.0f;
    public int voiceLimit;
    public List<AudioSource> activeVoices = new();
}


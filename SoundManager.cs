using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Toolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundBUS
{
    Master,
    Ambient,
    Environment,
    SFX,
    UI,
    Voice
}

[System.Serializable]
public class BusInfo
{
    public SoundBUS soundBus;
    public AudioMixer audioMixer;
    public float individualVolume = 1.0f;
    public int activeVoiceCount = 0;
    public int voiceLimit;
    public Queue<AudioSource> activeVoices = new Queue<AudioSource>();


    public BusInfo(SoundBUS soundBus, AudioMixer audioMixer, int voiceLimit)
    {
        this.soundBus = soundBus;
        this.audioMixer = audioMixer;
        this.voiceLimit = voiceLimit;
    }
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] List<SoundGroup> soundGroupsList = null;
    [SerializeField] private List<BusInfo> soundBusInformationList;

    private readonly Dictionary<string, SoundGroup> soundGroups = new();
    private readonly Dictionary<SoundBUS, BusInfo> busInfo = new();

    public BusInfo GetBUSControl(SoundBUS bus)
    {
        return busInfo[bus];
    }

    protected override void Awake()
    {
        base.Awake();

        foreach (BusInfo control in soundBusInformationList)
        {
            busInfo[control.soundBus] = control;
        }
    }

    private void Start()
    {
        foreach (SoundGroup soundGroup in soundGroupsList)
        {
            soundGroups[soundGroup.gameObject.name] = soundGroup;
        }
    }

    public void PlaySound(string soundGroupName)
    {
        PlaySoundInternal(soundGroupName, null);
    }

    public void PlaySound(string soundGroupName, Transform location)
    {
        PlaySoundInternal(soundGroupName, location);
    }

    private void PlaySoundInternal(string soundGroupName, Transform location)
    {
        if (soundGroups.TryGetValue(soundGroupName, out SoundGroup soundGroup))
        {
            BusInfo info = busInfo[soundGroup.SoundBUS];

            // Stop the oldest active voice and dequeue it, if voice limit is reached.
            if (info.activeVoiceCount >= info.voiceLimit && info.activeVoices.Count > 0)
            {
                AudioSource toStop = info.activeVoices.Dequeue();
                toStop.Stop();
                info.activeVoiceCount--;
            }

            AudioSource source = soundGroup.GetAvailableSource();
            if (source != null)
            {
                info.activeVoiceCount++;
                info.activeVoices.Enqueue(source);  // Enqueue the new active voice.
                soundGroup.OnAudioSourceStopped += HandleAudioSourceStopped;

                if (location != null)
                {
                    source.transform.position = location.position;
                }

                source.Play();
            }
        }
    }


    private void HandleAudioSourceStopped(SoundGroup soundGroup)
    {
        SoundBUS bus = soundGroup.SoundBUS;
        BusInfo info = busInfo[bus];

        if (info.activeVoices.Count > 0)
        {
            AudioSource toDequeue = info.activeVoices.Dequeue();
        }

        info.activeVoiceCount--;
        soundGroup.OnAudioSourceStopped -= HandleAudioSourceStopped;
    }

    public void SetBusVolume(SoundBUS bus, float volume)
    {
        BusInfo control = busInfo[bus];
        control.individualVolume = volume;
        if (bus == SoundBUS.Master)
        {
            foreach (BusInfo individualControl in busInfo.Values)
            {
                if (individualControl.soundBus != SoundBUS.Master)
                {
                    individualControl.audioMixer.SetFloat("Volume", Mathf.Log10(control.individualVolume * volume) * 20);
                }
            }
        }
        control.audioMixer.SetFloat("Volume", Mathf.Log10(volume) * 20);
    }

#if UNITY_EDITOR
    [InfoBox("This is a simple helper method for creating sound groups in bulk. Usage:\n\n" +
        "1) Click \"Lock Inspector\" while the game object with the SoundManager.cs component is selected\n" +
        "2) Select multiple sound group folders in the Project view.\n" +
        "3) Click this button.\n\n" +
        "A new game object with the same name as each respective folder will be created. This new game object will have a SoundGroup.cs attached.")]
    [Button("Create game objects for sound groups")]
    public void CreateChildObjects()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

        // Sort by name
        selectedAssets = selectedAssets.OrderBy(asset => asset.name).ToArray();

        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.Log("Invalid asset.");
                continue;
            }

            GameObject newObject = new(asset.name);
            newObject.AddComponent<AudioSource>();
            newObject.transform.SetParent(transform);
        }
    }
#endif

}


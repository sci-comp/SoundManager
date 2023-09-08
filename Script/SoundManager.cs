using Sirenix.OdinInspector;
using System.Collections.Generic;
using Toolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] List<SoundGroup> soundGroupsList = null;
    [SerializeField] private List<SoundBusInfo> soundBusInfoList;

    private readonly Dictionary<string, SoundGroup> soundGroups = new();
    private readonly Dictionary<SoundBUS, SoundBusInfo> allBusInfo = new();

    protected override void Awake()
    {
        base.Awake();

        foreach (SoundBusInfo info in soundBusInfoList)
        {
            allBusInfo[info.soundBus] = info;
        }
    }

    private void Start()
    {
        foreach (SoundGroup soundGroup in soundGroupsList)
        {
            soundGroups[soundGroup.gameObject.name] = soundGroup;
        }
    }

    public SoundBusInfo GetBUSInfoFromList(SoundBUS bus)
    {
        foreach (SoundBusInfo info in soundBusInfoList)
        {
            if (info.soundBus == bus)
            {
                return info;
            }
        }

        return null;
    }

    public void HandleAudioSourceStopped(SoundGroup soundGroup, AudioSource src)
    {
        SoundBUS bus = soundGroup.SoundBUS;
        SoundBusInfo busInfo = allBusInfo[bus];

        if (busInfo.activeVoices.Count > 0)
        {
            busInfo.activeVoices.Remove(src);
        }
        else
        {
            Debug.LogWarning("HandleAudioSourceStopped was invoked, but we have no actives voice.");
        }

        //soundGroup.OnAudioSourceStopped -= HandleAudioSourceStopped;
    }

    public void SetBusVolume(SoundBUS bus, float volume)
    {
        // TODO: This method is untested.
        SoundBusInfo control = allBusInfo[bus];
        control.individualVolume = volume;
        control.audioMixer.SetFloat("Volume", Mathf.Log10(volume) * 20);
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
            SoundBusInfo busInfo = allBusInfo[soundGroup.SoundBUS];


            if (busInfo.activeVoices.Count >= busInfo.voiceLimit && busInfo.activeVoices.Count > 0)
            {
                AudioSource toStop = busInfo.activeVoices[0];
                toStop.Stop();
                busInfo.activeVoices.RemoveAt(0);
            }

            (AudioSource source, bool recycled) = soundGroup.GetAvailableSource();

            if (source != null)
            {
                if (recycled)
                {
                    busInfo.activeVoices.Remove(source);
                    busInfo.activeVoices.Add(source);
                }
                else
                {
                    busInfo.activeVoices.Add(source);
                }

                //soundGroup.OnAudioSourceStopped += HandleAudioSourceStopped;

                if (location != null)
                {
                    source.transform.position = location.position;
                }

                source.Play();
            }
            else
            {
                Debug.LogWarning("Available audio source not found. Sound wasn't played: " + soundGroupName);
            }
        }
    }

#if UNITY_EDITOR

    [Title("Editor-time helper", "For batch creation of sound groups", TitleAlignments.Centered)]
    [InfoBox("Sound groups are pools that contain variations of a sound. Each sound group is represented by a folder that contains AudioClip variations.\n\n " +
        "Usage:\n" +
        "1) Click \"Lock Inspector\" while the scene game object with the SoundManager.cs component is selected.\n" +
        "2) Select multiple sound group folders in the Project view.\n" +
        "3) Click this button.\n\n" +
        "For each selected folder, a new game object with the same name will be created with SoundGroup.cs attached. To each of these game objects, we create child game objects with AudioSource components attached. Default settings for the AudioSource components will be set.")]
    [SerializeField] AudioMixerGroup audioMixerGroupForBatch = null;
    [SerializeField] bool use3DSoundForBatch = false;
    [Button("Create game objects for sound groups")]
    public void CreateChildObjects()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

        // Sort by name
        //selectedAssets = selectedAssets.OrderBy(asset => asset.name).ToArray();
        System.Array.Sort(selectedAssets, (a, b) => string.Compare(a.name, b.name));

        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.Log("Invalid asset.");
                continue;
            }

            // Create sound group object
            GameObject soundGroupObject = new("sfx_" + asset.name);
            SoundGroup soundGroup = soundGroupObject.AddComponent<SoundGroup>();
            soundGroupObject.transform.SetParent(transform);

            // Create audio source objects
            string[] assetPaths = AssetDatabase.FindAssets("t:AudioClip", new[] { assetPath });
            foreach (string assetGUID in assetPaths)
            {
                string childAssetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                string childAssetName = System.IO.Path.GetFileNameWithoutExtension(childAssetPath);

                AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(childAssetPath);

                // Create empty child game objects
                GameObject audioSourceObject = new(childAssetName);
                audioSourceObject.transform.SetParent(soundGroupObject.transform);
                                
                AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();

                audioSource.clip = audioClip;
                audioSource.outputAudioMixerGroup = audioMixerGroupForBatch;
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                audioSource.volume = 1.0f;
                audioSource.pitch = 1.0f;
                audioSource.spatialBlend = use3DSoundForBatch ? 1.0f : 0.0f;

                soundGroup.AudioSources.Add(audioSource);
            }
        }
    }
#endif

}


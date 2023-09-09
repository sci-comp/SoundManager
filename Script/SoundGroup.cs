using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Audio;

public class SoundGroup : MonoBehaviour
{
    [SerializeField] bool use3DSound = false;
    [SerializeField] Vector2 varyPitch = new(0.95f, 1.05f);
    [SerializeField] Vector2 varyVolume = new(0.94f, 1.0f);
    [SerializeField] SoundBUS soundBUS = SoundBUS.SFX;
    [SerializeField] List<AudioSource> audioSources = new();

    private readonly Dictionary<AudioSource, Coroutine> sourceCoroutines = new();

    public List<AudioSource> AudioSources => audioSources;
    public SoundBUS SoundBUS => soundBUS;
    public bool Use3DSound => use3DSound;

    private Queue<AudioSource> availableSources;
    private List<AudioSource> activeSources;

    public void Stop(AudioSource src)
    {
        src.Stop();
        StopCoroutine(sourceCoroutines[src]);
        activeSources.Remove(src);
        availableSources.Enqueue(src);
        SoundManager.Instance.HandleAudioSourceStopped(this, src);
    }

    private void Start()
    {
        availableSources = new(audioSources);
        activeSources = new();
    }

    public (AudioSource, SoundGroup) GetAvailableSource()
    {
        AudioSource src;

        if (availableSources.Count > 0)
        {
            src = availableSources.Dequeue();
            src.enabled = true;
            activeSources.Add(src);
        }
        else if (activeSources.Count > 0)
        {
            src = activeSources[0];
            src.enabled = true;  // Should already be enabled
            src.Stop();
            SoundManager.Instance.HandleAudioSourceStopped(this, src);
            StopCoroutine(sourceCoroutines[src]);
            activeSources.RemoveAt(0);
            activeSources.Add(src);
        }
        else
        {
            Debug.LogError("No active or available audio sources. This is not logical.");
            return (null, this);
        }

        src.pitch = Random.Range(varyPitch.x, varyPitch.y);
        src.volume = Random.Range(varyVolume.x, varyVolume.y);

        sourceCoroutines[src] = StartCoroutine(WaitForAudioToEnd(src));

        return (src, this);
    }

    private IEnumerator WaitForAudioToEnd(AudioSource src)
    {
        src.Play();

        yield return new WaitUntil(() => !src.isPlaying);

        src.enabled = false;
        activeSources.Remove(src);
        availableSources.Enqueue(src);

        SoundManager.Instance.HandleAudioSourceStopped(this, src);
    }

#if UNITY_EDITOR
    [InfoBox("This is a simple helper method for creating AudioSource components for a given sound group. Usage:\n\n" +
        "1) Click \"Lock Inspector\" while the game object with the SoundGroup.cs component is selected\n" +
        "2) Select multiple audio clip assets in the Project view.\n" +
        "3) Click this button.\n\n" +
        "A new game object with the same name as each respective audio clip will be created. This new game object will have an AudioSource.cs component attached, and the AudioSource will be initialized with default values and references from the sound group.")]
    [Button("Create game objects for audio sources")]
    public void CreateChildObjects()
    {
        SoundBusInfo busInfo = SoundManager.Instance.GetBUSInfoFromList(soundBUS);
        AudioMixerGroup[] groups = busInfo.audioMixer.FindMatchingGroups(string.Empty);
        AudioMixerGroup audioMixerGroup = null;

        foreach (var group in groups)
        {
            if (group.name == "Master")
            {
                audioMixerGroup = group;
            }
        }

        if (audioMixerGroup == null ) { 
            Debug.Log("Null mixer: " + audioMixerGroup);
        }

        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        System.Array.Sort(selectedAssets, (a, b) => string.Compare(a.name, b.name));

        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.Log("Invalid asset.");
                continue;
            }

            if (asset is AudioClip audioClip)
            {
                GameObject newObject = new(asset.name);
                AudioSource audioSource = newObject.AddComponent<AudioSource>();
                newObject.transform.SetParent(transform);

                audioSource.clip = audioClip;
                audioSource.outputAudioMixerGroup = audioMixerGroup;
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                audioSource.volume = 1.0f;
                audioSource.pitch = 1.0f;
                audioSource.spatialBlend = use3DSound ? 1.0f : 0.0f;

                audioSources.Add(audioSource);
            }
                
        }
    }
#endif

}


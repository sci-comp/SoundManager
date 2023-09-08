using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;

public class SoundGroup : MonoBehaviour
{
    public delegate void AudioSourceStoppedHandler(SoundGroup soundGroup);
    public event AudioSourceStoppedHandler OnAudioSourceStopped;

    [SerializeField] bool use3DSound = false;
    [SerializeField] Vector2 varyPitch = new(0.95f, 1.05f);
    [SerializeField] Vector2 varyVolume = new(0.94f, 1.0f);
    [SerializeField] SoundBUS soundBUS = SoundBUS.SFX;
    [SerializeField] List<AudioSource> audioSources;

    public List<AudioSource> AudioSources => audioSources;
    public SoundBUS SoundBUS => soundBUS;
    public bool Use3DSound => use3DSound;

    private Queue<AudioSource> availableSources;
    private List<AudioSource> activeSources;

    private void Start()
    {
        availableSources = new(audioSources);
        activeSources = new();
    }

    public AudioSource GetAvailableSource()
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
            src.Stop();
            src.enabled = true;
            activeSources.RemoveAt(0);
            activeSources.Add(src);
        }
        else
        {
            Debug.LogError("No active or available audio sources. This is not logical.");
            return null;
        }

        src.pitch = Random.Range(varyPitch.x, varyPitch.y);
        src.volume = Random.Range(varyVolume.x, varyVolume.y);

        StartCoroutine(WaitForAudioToEnd(src));

        return src;
    }

    private IEnumerator WaitForAudioToEnd(AudioSource src)
    {
        src.Play();

        yield return new WaitUntil(() => !src.isPlaying);

        src.enabled = false;
        activeSources.Remove(src);
        availableSources.Enqueue(src);

        OnAudioSourceStopped?.Invoke(this);
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
        }
    }
#endif
}


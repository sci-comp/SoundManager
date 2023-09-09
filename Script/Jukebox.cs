using UnityEngine;
using System.Collections.Generic;

public class Jukebox : MonoBehaviour
{
    [System.Serializable]
    public class MusicGroup
    {
        public string name;
        public AudioClip[] clips;
    }

    [SerializeField] NextEventTimer nextEventTimer;

    public List<MusicGroup> musicGroups = new();
    public AudioSource musicSource;
    public AudioSource environmentSource;

    private List<AudioClip> playlist;

    private void Awake()
    {
        playlist ??= new();
    }

    private void Start()
    {
        nextEventTimer.EventTriggered += SleepOver;
    }

    public void SleepOver()
    {
        ConstructRandomPlaylist(3);
    }

    private void ConstructRandomPlaylist(int numSongs)
    {
        playlist.Clear();

        for (int i = 0; i < numSongs; i++)
        {
            int groupIndex = Random.Range(0, musicGroups.Count);
            int clipIndex = Random.Range(0, musicGroups[groupIndex].clips.Length);

            playlist.Add(musicGroups[groupIndex].clips[clipIndex]);
        }

        // Code to play the playlist can go here
    }

    public void OnPlaylistEnded()
    {
        nextEventTimer.Reset();
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

}

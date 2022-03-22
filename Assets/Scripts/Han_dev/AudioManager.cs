using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource catAudioSource;
    public AudioSource BGMAudioSource;
    public AudioData globalAudioData;

    void Start()
    {
        InitializeAudioData();
        BGMAudioSource.Play();
    }
    void InitializeAudioData()
    {
        // BGM
        BGMAudioSource = GetComponent<AudioSource>();
        BGMAudioSource.clip = globalAudioData.BGM;
        BGMAudioSource.loop = true;

        if (!catAudioSource)
            catAudioSource = GameObject.FindGameObjectWithTag("Cat").GetComponent<AudioSource>();

    }
    void PlayCatAudio(int index)
    {

        if (catAudioSource.isPlaying) return;

        var catAudioClips = globalAudioData.AudioClip;
        catAudioSource.clip = catAudioClips[index];
        catAudioSource.Play();
    }
    public void PlayRandomCatAudio()
    {
        if (catAudioSource.isPlaying) return;

        var catAudioClips = globalAudioData.AudioClip;
        int index = (int)Random.Range(0, 4);
        catAudioSource.clip = catAudioClips[index];
        catAudioSource.Play();
    }
}

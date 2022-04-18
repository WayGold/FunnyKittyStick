using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Reflection;




public class AudioItem : MonoBehaviour
{

    public enum AudioItemType { NORMAL, RANDOM, AMBIENT}

    [Header("Audio Attributes")]
    public List<AudioSource> asGroup;
    public AudioSource asReference;

    public int asIndex;
    public int asCount = 1;
    public AudioClip[] clipGroup;
    public AudioItemType _type;
    public float ambientFadeInTime;
    public float ambientHoldTime;
    public float ambientFadeOutTime;

    [Header("Debug")]
    [InspectorButton("Play")]
    public bool PlayAudio;


    // Start is called before the first frame update
    void Start()
    {

        TryGetComponent<AudioSource>(out asReference);

        // Create AudioSource of asCount number
        for(int i = 0; i < asCount; ++i)
        {
            CreateNewAudioSource();
        }

        
        
    }


    void CopyAudioSourceValue(AudioSource as_from, AudioSource as_to)
    {
        // Manually Copy every field
        as_to.outputAudioMixerGroup = as_from.outputAudioMixerGroup;
        as_to.mute = as_from.mute;
        as_to.bypassEffects = as_from.bypassEffects;
        as_to.bypassListenerEffects = as_from.bypassListenerEffects;
        as_to.bypassReverbZones = as_from.bypassReverbZones;
        as_to.playOnAwake = as_from.playOnAwake;
        as_to.loop = as_from.loop;
        as_to.priority = as_from.priority;
        as_to.volume = as_from.volume;
        as_to.pitch = as_from.pitch;
        as_to.panStereo = as_from.panStereo;
        as_to.spatialBlend = as_from.spatialBlend;
        as_to.reverbZoneMix = as_from.reverbZoneMix;

    }


    void CreateNewAudioSource()
    {
        // Create A New AS
        var _as = gameObject.AddComponent<AudioSource>();

        // Copy initial audioSource setting if any
        if (asReference != null) CopyAudioSourceValue(asReference, _as);

        // Assign AS Attributes
        switch (_type)
        {

            // One Shot Audio
            case AudioItemType.NORMAL:
                _as.clip = clipGroup[0];
                _as.playOnAwake = false; // Not Necessary
                break;


            // Random Audio
            case AudioItemType.RANDOM:
                _as.clip = GetRandomAudioClip();
                _as.playOnAwake = false; // Not Necessary
                break;
        

            // Continues Seamless Audio
            case AudioItemType.AMBIENT:
                _as.clip = clipGroup[0];
                _as.volume = 0;
                _as.loop = true;
                _as.Play();
                break;

            
        }


        // Add to list to track
        asGroup.Add(_as);



    }


    public AudioSource GetCurrentAudioSource()
    {
        return asGroup[asIndex];
    }


    // Make Audio Obvious


    public void Play()
    {


        // Check if current Index is available
        if (GetCurrentAudioSource().isPlaying)
        {
            // update index
            asIndex = (asIndex + 1) % asGroup.Count;
        }


        switch (_type)
        {
           

            // Continues Seamless Audio
            case AudioItemType.AMBIENT:
                StartCoroutine(PlayAmbient());
                break;

            // Random
            case AudioItemType.RANDOM:

                var currentClip = GetCurrentAudioSource().clip;
                var randomClip = GetRandomAudioClip();

                // Try to get a different audioClip than last time
                while(currentClip == randomClip)
                {
                    randomClip = GetRandomAudioClip();
                }
                GetCurrentAudioSource().clip = randomClip;


                GetCurrentAudioSource().Play();
                break;

            // Otherwise, just normal play it
            default:
                GetCurrentAudioSource().Play();
                break;
        }
    }






    private AudioClip GetRandomAudioClip()
    {
        var index = (int)Random.Range(0, clipGroup.Length);
        var randomClip = clipGroup[index];
        return randomClip;
    }
    
    private IEnumerator PlayAmbient()
    {
        StartCoroutine(FadeInAudioSource(GetCurrentAudioSource(), ambientFadeInTime));
        yield return new WaitForSeconds(ambientHoldTime);
        StartCoroutine(FadeOutAudioSource(GetCurrentAudioSource(), ambientFadeOutTime));
    }







    // TOOLS


    // Fade AudioSource
    public static IEnumerator FadeOutAudioSource(AudioSource audioSource, float FadeTime)
    {

        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeInAudioSource(AudioSource audioSource, float FadeTime)
    {
        float startVolume = 0.2f;

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = 1f;
    }

    // Fade AudioGroup;

    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    public void SetAudioMixerValue(AudioMixer audioMixer, string exposedParam, float value)
    {
        audioMixer.SetFloat(exposedParam, value);
    }


}

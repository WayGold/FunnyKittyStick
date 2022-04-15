using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioItem : MonoBehaviour
{

    public enum AudioItemType { NORMAL, RANDOM, AMBIENT}

    [Header("Audio Attributes")]
    public List<AudioSource> asGroup;
    public int asIndex;
    public int asCount;
    public AudioClip[] clipGroup;
    public AudioItemType _type;
    public float ambientTime = 2f;


    // Start is called before the first frame update
    void Start()
    {
        // Create AudioSource of asCount number
        for(int i = 0; i < asCount; ++i)
        {
            CreateNewAudioSource();
        }
    }


    void CreateNewAudioSource()
    {
        // Create A New AS
        var _as = gameObject.AddComponent<AudioSource>();

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
                var index = (int)Random.Range(0, clipGroup.Length - 1);
                var randomClip = clipGroup[index];
                _as.clip = randomClip;
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


    // Make Audio Obvious


    public void Play()
    {


        // Check if current Index is available
        if (asGroup[asIndex].isPlaying)
        {
            // update index
            asIndex = (asIndex + 1) % asGroup.Count;
        }


        switch (_type)
        {
           

            // Continues Seamless Audio
            case AudioItemType.AMBIENT:
                StartCoroutine(PlayAmbient(ambientTime));
                break;


            // Otherwise, just normal play it
            default:
                asGroup[asIndex].Play();
                break;
        }
    }


    
    private IEnumerator PlayAmbient(float time)
    {
        FadeInAudioSource(asGroup[asIndex], 0.3f);
        yield return new WaitForSeconds(time);
        FadeOutAudioSource(asGroup[asIndex], 1f);
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

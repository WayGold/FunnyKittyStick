using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;


public class TempGameManager : MonoBehaviour
{
    #region SINGLETON

    private static TempGameManager _Instance;
    public static TempGameManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }

        else
        {
            // DEBUG: In case multiple instance are loaded
            if (_Instance != this)
            {
                Debug.Log("Multiple Instance Detected and Destroy Extra Instance.");
            }
        }

        // DontDestroyOnLoad(gameObject);
    }

    #endregion SINGLETON



    // Start is called before the first frame update






    [Header("Audio")]
    private AudioSource BGMAudioSource;
    public AudioSource CatAudioSource;
    public AudioSource SFXAudioSource;
    public AudioData globalAudioData;
    public bool isPlayedBGM;


    void InitializeAudioData()
    {
        // BGM
        BGMAudioSource = GetComponent<AudioSource>();
        BGMAudioSource.clip = globalAudioData.BGM;
        BGMAudioSource.loop = true;

        // Audio


    }

    void CheckPlayBGM()
    {
        if (!isPlayedBGM)
        {
            BGMAudioSource.Play();
            isPlayedBGM = true;
        }
    }

    void PlayCatAudio(int index)
    {

        if (CatAudioSource.isPlaying) return;

        var catAudioClips = globalAudioData.AudioClip;
        CatAudioSource.clip = catAudioClips[index];
        CatAudioSource.Play();
        
    }

    void PlayRandomCatAudio()
    {


        if (CatAudioSource.isPlaying) return;

        var catAudioClips = globalAudioData.AudioClip;
        int index = (int)Random.Range(0, 4);
        CatAudioSource.clip = catAudioClips[index];
        CatAudioSource.Play();
    }

    public void PlaySFXAttack()
    {

        if (SFXAudioSource.isPlaying) return;
        SFXAudioSource.clip = globalAudioData.AudioClip[4];
        SFXAudioSource.Play();
    }

    public void PlaySFXJump()
    {
        if (SFXAudioSource.isPlaying) return;
        SFXAudioSource.clip = globalAudioData.AudioClip[5];
        SFXAudioSource.Play();
    }


    [Header("Cat Emoji UI")]
    public Animator catUIAnimator;
    public Animator fishUIAnimator;
    public RectTransform catUITransform;


    void PlayUIAnimation(Animator _animator,string boolName, float duration)
    {
        _animator.SetBool(boolName, true);
        StartCoroutine(HoldUIAnimation(_animator, boolName, duration));
        
    }

    IEnumerator HoldUIAnimation(Animator _animator, string boolName, float duration)
    {
        yield return new WaitForSeconds(duration);
        _animator.SetBool(boolName, false);
    }




    // --- Public Event

    public void OnCatAttack()
    {
        // PlayCatAudio(2);
        // PlaySFXAttack();
        PlayUIAnimation(catUIAnimator, "Trying", 2);
    }

    public void OnCatJumpForward()
    {
        PlayCatAudio(0);
        // PlaySFXJump();
        PlayUIAnimation(fishUIAnimator, "Highlight", 2f);
    }

    public void OnCatSit()
    {
        PlayCatAudio(2);
        PlayUIAnimation(catUIAnimator, "Event", 1.2f);
    }

    public void OnCatJumpUp()
    {
        // PlayCatAudio(3);
        // PlaySFXJump();
        PlayUIAnimation(catUIAnimator, "Event", 1.5f);
    }

    public void OnCatStand()
    {
        // PlayCatAudio(1);
        PlayUIAnimation(catUIAnimator, "Trying", 1.5f);
    }



    [Header("Joycon Input")]
    private Joycon joycon;

    void CheckJoyConInput()
    {
        if (joycon.GetButtonDown(Joycon.Button.DPAD_LEFT))
        {
            PlayRandomCatAudio();
        }
    }


    void Start()
    {
        joycon = JoyconManager.Instance.j[0];
    }


    // Update is called once per frame
    void Update()
    {
        CheckJoyConInput();
    }






}

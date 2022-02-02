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
    void Start()
    {
       //  InitializeAudioData();
       //  CheckPlayBGM();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    [Header("Audio")]
    private AudioSource BGMAudioSource;
    public AudioSource CatAudioSource;
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
        var catAudioClips = globalAudioData.AudioClip;
        CatAudioSource.clip = catAudioClips[index];
        if(!CatAudioSource.isPlaying) CatAudioSource.Play();
        
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
        //PlayCatAudio(4);
        PlayUIAnimation(catUIAnimator, "Trying", 2);
    }

    public void OnCatJumpForward()
    {
        PlayCatAudio(1);
        PlayUIAnimation(fishUIAnimator, "Highlight", 2f);
    }

    public void OnCatSit()
    {
        PlayCatAudio(2);
        PlayUIAnimation(catUIAnimator, "Event", 1.2f);
    }

    public void OnCatJumpUp()
    {
        //PlayCatAudio(3);
        // PlayUIAnimation(catUIAnimator, "Event", 1.5f);
    }

    public void OnCatStand()
    {
        PlayCatAudio(1);
        PlayUIAnimation(catUIAnimator, "Trying", 1.5f);
    }




    
   
}

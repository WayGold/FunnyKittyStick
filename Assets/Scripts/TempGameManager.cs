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

        DontDestroyOnLoad(gameObject);
    }

    #endregion SINGLETON



    // Start is called before the first frame update
    void Start()
    {
        InitializeAudioData();
        CheckPlayBGM();
    }

    // Update is called once per frame
    void Update()
    {
        TestEmoji();
    }



    [Header("Audio")]
    private AudioSource BGMAudioSource;
    public AudioData globalAudioData;
    public bool isPlayedBGM;


    void InitializeAudioData()
    {
        // BGM
        BGMAudioSource = GetComponent<AudioSource>();
        BGMAudioSource.clip = globalAudioData.BGM;
        BGMAudioSource.loop = true;
    }

    void CheckPlayBGM()
    {
        if (!isPlayedBGM)
        {
            BGMAudioSource.Play();
            isPlayedBGM = true;
        }
    }


    [Header("Cat Emoji UI")]
    public Animator catUIAnimator;
    public Animator fishUIAnimator;
    public RectTransform catUITransform;


    void PlayUIAnimation(string boolName, float duration)
    {
        catUIAnimator.SetBool(boolName, true);
        StartCoroutine(HoldUIAnimation(boolName, duration));
        
    }

    IEnumerator HoldUIAnimation(string boolName, float duration)
    {
        yield return new WaitForSeconds(duration);
        catUIAnimator.SetBool(boolName, false);
    }

    void TestEmoji()
    {

        // Only For Testing
        if (Input.GetKeyDown(KeyCode.P))
        {
             PlayUIAnimation("Trying", 3);
        }
    }
    
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneMgr : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public Animator _fader;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            videoPlayer.playbackSpeed = 2.5f;
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            videoPlayer.playbackSpeed = 1.5f;
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            _switchToMainGame();
        }
        videoPlayer.loopPointReached += EndReached;
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        _fader.SetTrigger("FadeOut");
    }

    public void _switchToSelection()
    {
        SceneManager.LoadScene("CatSelection 1");
    }

    public void _switchToMainGame()
    {
        SceneManager.LoadScene("mainGame");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WiimoteApi;

public class MenuControlSystem : MonoBehaviour
{
    [SerializeField] public string mainGameLevel;
    [SerializeField] public string mainMenuLevel;

    public void LoadNewGameLevelCallback()
    {
        SceneManager.LoadScene(mainGameLevel);
    }

    public void LoadMainMenuLevelCallback()
    {
        SceneManager.LoadScene(mainMenuLevel);
    }

    public void QuitGameCallback()
    {
        Application.Quit();
    }
}
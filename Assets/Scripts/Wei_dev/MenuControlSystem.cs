using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControlSystem : MonoBehaviour
{
    [SerializeField] public string mainGameLevel;
    [SerializeField] public string mainMenuLevel;

    // Start is called before the first frame update
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
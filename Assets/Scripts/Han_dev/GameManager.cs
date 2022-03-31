using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  
    //Use to manage the game process, music etc.
    void Start()
    {
        
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
            RestartGame();
    }
   
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

   
}

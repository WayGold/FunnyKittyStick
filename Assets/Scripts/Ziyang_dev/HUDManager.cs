using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    public UIManager uiManager;

    public List<Collectible> collectibles;

    Slider HeartSlider;

    public float GrowthNum;

    public bool growth;
    public bool isCompelete;

    public float target_value;

    public Image FadeOutImage;

    public int interableCount;

    // Start is called before the first frame update
    void Start()
    {
        collectibles = GameObject.FindObjectsOfType<Collectible>().ToList();
        HeartSlider = GetComponentInChildren<Slider>();
        HeartSlider.value = 0;
        GrowthNum = 1f/(collectibles.Count + interableCount);
        growth = false;
        isCompelete = false;
        target_value = 0;
    }

    public void ColletItem()
    {
        target_value += GrowthNum;
        growth = true;
        uiManager.CatLike();
    }

    void CompleteLevel()
    {
        Debug.Log("Complete!");
        isCompelete = true;
        
        StartCoroutine(BlackTheScreen());
    }

    float a = 0;
    IEnumerator BlackTheScreen()
    {
        while(true)
        {
            a += 0.005f;
            FadeOutImage.color = new Color(0, 0, 0, a);
            if (FadeOutImage.color.a >= 1)
                SceneManager.LoadScene("End");
            yield return null;
        }
    }

    void Update()
    {
        if (growth)
        {
            HeartSlider.value += 0.003f;
            if(HeartSlider.value >= target_value)
            {
                growth = false;
                if (HeartSlider.value >= GrowthNum * (collectibles.Count + interableCount))
                {
                    CompleteLevel();
                }
            }
            if (HeartSlider.value >= 1)
            {
                CompleteLevel();
            }
        }
    }
}

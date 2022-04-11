using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HUDManager : MonoBehaviour
{


    public List<Collectible> collectibles;

    Slider HeartSlider;

    float GrowthNum;

    bool growth;

    float target_value;

    // Start is called before the first frame update
    void Start()
    {
        collectibles = GameObject.FindObjectsOfType<Collectible>().ToList();
        HeartSlider = GetComponentInChildren<Slider>();
        HeartSlider.value = 0;
        GrowthNum = 1f/collectibles.Count;
        growth = false;
        target_value = 0;
    }

    public void ColletItem()
    {
        target_value += GrowthNum;
        growth = true;

    }

    void CompleteLevel()
    {
        Debug.Log("Complete!");
    }

    // Update is called once per frame
    void Update()
    {
        if (growth)
        {
            HeartSlider.value += 0.003f;
            if(HeartSlider.value >= target_value)
            {
                growth = false;
                if (HeartSlider.value >= GrowthNum * collectibles.Count)
                {
                    CompleteLevel();
                }
            }
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatSelector : MonoBehaviour
{
    [SerializeField] List<GameObject> cats;
    [SerializeField] GameObject jumpTarget;
    [SerializeField] GameObject fish;

    private bool selected = false;
    private GameObject selectedCat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
            return;

        foreach(GameObject cat in cats)
        {
            if(isInRadius(cat.transform.position, fish.transform.position, 3))
            {
                selected = true;
                selectedCat = cat;

                // Jump Out Of The Box, Enable Jump Out Animator
                cat.GetComponent<Animator>().enabled = true;

                break;
            }
        }
    }

    bool isInRadius(Vector3 pos_a, Vector3 pos_b, float radius)
    {
        if ((pos_b.x - pos_a.x) * (pos_b.x - pos_a.x) + (pos_b.z - pos_a.z) * (pos_b.z - pos_a.z) <= radius * radius)
            return true;

        return false;
    }
}

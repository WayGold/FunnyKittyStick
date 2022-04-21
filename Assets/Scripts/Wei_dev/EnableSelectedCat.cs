using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableSelectedCat : MonoBehaviour
{
    [SerializeField] public GameObject cat_white;
    [SerializeField] public GameObject cat_short;
    [SerializeField] public GameObject cat_grey;

    private bool first_entrance;

    private void Awake()
    {
        switch (CatSelection.CAT_SELECTED)
        {
            case 0:
                cat_grey.SetActive(true);
                break;
            case 1:
                cat_short.SetActive(true);
                break;
            case 2:
                cat_white.SetActive(true);
                break;
            default:
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

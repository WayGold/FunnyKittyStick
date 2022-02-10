using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTargetPosition : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Wand is located at " + GetComponent<RectTransform>().position);

    }
}

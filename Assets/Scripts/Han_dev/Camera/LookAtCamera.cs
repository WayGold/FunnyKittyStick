using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Start()
    {
           
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);       
    }
}

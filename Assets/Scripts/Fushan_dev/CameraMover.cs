using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{


    [Header("Pan Shoot Points")]
    public Transform point1;
    public Transform point2;
    public float totaltime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Lerp();   
    }

    void Lerp()
    {
        // Current Progress
        var progress = Time.timeSinceLevelLoad / totaltime;
        if (progress > 1) return;

        // Lerp Btw 2 poitns
        var currentPos = point1.position * progress + point2.position * (1 - progress);
        transform.position = currentPos;

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDemo : MonoBehaviour
{

    public static WebCamTexture cameraTexture;

    // Start is called before the first frame update
    void Start()
    {
        if (cameraTexture == null)
            cameraTexture = new WebCamTexture();

        GetComponent<Renderer>().material.mainTexture = cameraTexture;

        if (!cameraTexture.isPlaying)
            cameraTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

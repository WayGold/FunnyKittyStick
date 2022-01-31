using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDemo : MonoBehaviour
{

    public static WebCamTexture cameraTexture;

    // Start is called before the first frame update
    //void Start()
    //{
    //    if (cameraTexture == null)
    //        cameraTexture = new WebCamTexture();

    //    GetComponent<Renderer>().material.mainTexture = cameraTexture;

    //    if (!cameraTexture.isPlaying)
    //        cameraTexture.Play();
    //}

    public string deviceName;
    public WebCamTexture tex;
    // Use this for initialization  
    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            deviceName = devices[0].name;
            tex = new WebCamTexture(deviceName);
            
            gameObject.GetComponent<Renderer>().material.mainTexture = tex;
            tex.Play();
        }
        else
        {
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

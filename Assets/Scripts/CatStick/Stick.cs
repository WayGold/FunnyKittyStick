using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick : MonoBehaviour
{
    public Vector3 offset=new Vector3(90,-90,-90);

    void Update()
    {
        //Vector3 cameraAngles = new Vector3(Camera.main.transform.eulerAngles.x, 0, Camera.main.transform.eulerAngles.z);
        gameObject.transform.eulerAngles = (Camera.main.transform.eulerAngles) + offset;
    }
}

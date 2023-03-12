using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickRotate : MonoBehaviour
{
    Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 stickScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Debug.Log(90 * (stickScreenPos.x / Screen.width));
        
        transform.eulerAngles = new Vector3(-90 * (stickScreenPos.y / Screen.height) + 45, 90 * (stickScreenPos.x / Screen.width)-90,0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetController : MonoBehaviour
{
    public SerialController serialController;

    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();

        Debug.Log("Press A or Z to execute some actions");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnOn()
    {
        Debug.Log("Turning On!");
        serialController.SendSerialMessage("O");
    }

    public void TurnOff()
    {
        Debug.Log("Turning Off!");
        serialController.SendSerialMessage("P");
    }
}

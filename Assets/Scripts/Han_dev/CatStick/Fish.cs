using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    private MagnetController magnetController;

    Rigidbody rigibody;
    public float force;
    void Start()
    {
        rigibody = gameObject.GetComponent<Rigidbody>();
        if(GameObject.Find("ArduinoController"))
        {
            magnetController = GameObject.Find("ArduinoController").GetComponent<MagnetController>();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PullFish();
        }
    }

    public void PullFish()
    {
        rigibody.AddForce(0, force*-1, 0);
        Debug.Log("PullFish");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cat" && magnetController != null)
        {
            print("hit the cat!");

        }
    }

    public void TurnOnMagnet()
    {
        if(magnetController != null)
        {
            magnetController.TurnOn();
        }
    }

    public void TurnOffMagnet()
    {
        if (magnetController != null)
        {
            magnetController.TurnOff();
        }
    }
}

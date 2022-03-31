using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    Rigidbody rigibody;
    public float force;
    void Start()
    {
        rigibody = gameObject.GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            PullFish();
        }
    }

    public void PullFish()
    {
        rigibody.AddForce(0, force*-1, 0);
        Debug.Log("PullFish");
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="cat")
        {
            print("hit the cat!");
            //put code here, Sage

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 Force;
    Rigidbody MyRigibody;
    void Start()
    {
        MyRigibody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MyRigibody.AddForce(Force);
    }
}

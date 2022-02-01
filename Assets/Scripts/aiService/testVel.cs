using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testVel : MonoBehaviour
{
    public Rigidbody myRB;
    public Rigidbody catRB;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W)){
            myRB.velocity += new Vector3(1, 0, 0);
        }

        if(Input.GetKeyDown(KeyCode.S)){
            myRB.velocity += new Vector3(-1, 0, 0);
        }

        if(Input.GetKeyDown(KeyCode.UpArrow)){
            myRB.AddForce(new Vector3(0, 10, 0), ForceMode.Impulse);
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            catRB.AddForce(new Vector3(0, 10, 0), ForceMode.VelocityChange);
        }
    }
}

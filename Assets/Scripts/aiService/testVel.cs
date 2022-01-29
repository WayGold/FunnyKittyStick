using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testVel : MonoBehaviour
{
    public Rigidbody myRB;
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
    }
}

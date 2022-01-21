using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agent;
using MovementOutputs;

public class aiService : MonoBehaviour
{
    public aiService(){}
    // Start is called before the first frame update
    void Start(){}
    // Update is called once per frame
    void Update(){}

    KinematicSteeringOutput kinematicSeek(Rigidbody characterRB, Rigidbody targetRB, float maxSpeed){
        KinematicSteeringOutput retVal;
        float orientation;

        Vector3 seekVec = targetRB.position - characterRB.position;
        seekVec = Vector3.Normalize(seekVec);
        seekVec *= maxSpeed;
        // Ignore rotation on y axis
        orientation = Mathf.Atan2(seekVec.x, seekVec.z);

        retVal.linearVelocity = seekVec;
        retVal.rotVelocity = orientation;

        return retVal;
    }

    // DynamicSteeringOutput dynamicSeek(){
    //     DynamicSteeringOutput retVal;



    //     return retVal;
    // }
}

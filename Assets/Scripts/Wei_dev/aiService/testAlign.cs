using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIService;
using MovementOutputs;

public class testAlign : MonoBehaviour
{

    public Rigidbody agentRB;
    public Rigidbody targetRB;
    [SerializeField] public Rigidbody auxRB;

    public float maxSpeed;
    public float maxTargetSpeed;
    public float maxAcceleration;

    public float maxAngularAcceleration;
    public float maxRotation;

    public float targetRadius;
    public float slowRadius;
    public float timeToTarget;

    DynamicAlign dynamicAlign;
    DynamicArrive dynamicArrive;
    DynamicFace dynamicFace;
    DynamicLWYAG dynamicLWYAG;

    // Start is called before the first frame update
    void Start()
    {
        dynamicAlign = new DynamicAlign(agentRB, targetRB, maxAngularAcceleration, maxRotation);
        dynamicArrive = new DynamicArrive(agentRB, targetRB, maxAcceleration, maxSpeed, 0.5f, 2f);
        dynamicFace = new DynamicFace(agentRB, targetRB, auxRB, maxAngularAcceleration, maxRotation, targetRadius, slowRadius);
        dynamicLWYAG = new DynamicLWYAG(agentRB, auxRB, maxAngularAcceleration, maxRotation, targetRadius, slowRadius);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            targetRB.angularVelocity += new Vector3(0, 1, 0);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            targetRB.velocity += new Vector3(1, 0, 0);
        }

        DynamicSteeringOutput steering = dynamicLWYAG.getSteering();
        steering.linearAccel = dynamicArrive.getSteering().linearAccel;

        UpdateRigidBody(steering);
    }

    void UpdateRigidBody(DynamicSteeringOutput i_steering)
    {
        // Update position and orientation
        agentRB.position += agentRB.velocity * Time.deltaTime;
        agentRB.rotation = Quaternion.Euler(new Vector3(0, agentRB.rotation.eulerAngles.y + (agentRB.angularVelocity.y * Time.deltaTime) * Mathf.Rad2Deg, 0));
        
        // Update velocity and rotation.
        agentRB.velocity += i_steering.linearAccel * Time.deltaTime;
        agentRB.angularVelocity += new Vector3(0, i_steering.rotAccel * Time.deltaTime, 0);

        // Check for speeding and clip.
        if (Vector3.Magnitude(agentRB.velocity) > maxSpeed)
        {
            agentRB.velocity = Vector3.Normalize(agentRB.velocity);
            agentRB.velocity *= maxSpeed;
        }

        // Check for rot speeding and clip.
        if (agentRB.angularVelocity.y > maxRotation)
        {
            agentRB.angularVelocity = new Vector3(agentRB.angularVelocity.x, maxRotation, agentRB.angularVelocity.z);
        }
    }

}

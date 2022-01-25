using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementOutputs;
using static AIService.aiService;

public class agent : MonoBehaviour
{
    public Rigidbody agentRB;
    public Rigidbody targetRB;
    public float maxSpeed;
    public float maxRotationSpeed;
    public float idleRadius;
    public float maxTargetSpeed;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        KinematicSteeringOutput currentMovement;

        // Set a radius around the agent determining an idle range
        // Only head track + orientation matching 
        if (isInRadius(agentRB.position, targetRB.position, idleRadius))
        {
            currentMovement.linearVelocity = new Vector3(0, 0, 0);
            currentMovement.rotVelocity = 0;
        }
        else
        { currentMovement = kinematicSeek(agentRB, targetRB, maxSpeed); }

        // Fix y so the agent don't fly
        currentMovement.linearVelocity.y = 0;

        if (!isTooFast(targetRB, maxTargetSpeed))
        {
            Debug.Log("Not Too Fast With Target Velocity - " + targetRB.velocity);
            agentRB.velocity = currentMovement.linearVelocity;
        }
        {
            Debug.Log("Too Fast!");
        }
           


        Quaternion deltaRot = Quaternion.Euler(new Vector3(0, currentMovement.rotVelocity * Mathf.Rad2Deg, 0));
        agentRB.MoveRotation(deltaRot);
    }

    bool isInRadius(Vector3 agent, Vector3 target, float radius)
    {
        if ((target.x - agent.x) * (target.x - agent.x) + (target.z - agent.z) * (target.z - agent.z) <= radius)
            return true;

        return false;
    }

    bool isTooFast(Rigidbody target, float maxTargetSpeed)
    {
        if (target.velocity.magnitude > maxTargetSpeed)
            return true;
        return false;
    }
}

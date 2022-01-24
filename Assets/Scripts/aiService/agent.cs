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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        KinematicSteeringOutput currentMovement = kinematicSeek(agentRB, targetRB, maxSpeed);

        // Fix y so the agent don't fly
        currentMovement.linearVelocity.y = 0;
        agentRB.velocity = currentMovement.linearVelocity;

        Quaternion deltaRot = Quaternion.Euler(new Vector3(0, currentMovement.rotVelocity * Mathf.Rad2Deg, 0));
        agentRB.MoveRotation(deltaRot);
    }
}

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
    //public float maxRotationSpeed;
    public float idleRadius;
    public float maxTargetSpeed;

    private Animator _animator;
    private bool isSit = false;
    private bool toSeek = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        // _animator.SetTrigger("StandUp");
    }

    // Update is called once per frame
    void Update()
    {
        KinematicSteeringOutput currentMovement;
        currentMovement.linearVelocity = new Vector3(0, 0, 0);
        currentMovement.rotVelocity = 0;

        /* IDLE RANGE - AGENT MOVE NEAR TARGET */
        // Set a radius around the agent determining an idle range
        // Only head track + orientation matching, no seeking, attack the target
        if (isInRadius(agentRB.position, targetRB.position, idleRadius))
        {
            toSeek = false;
            _animator.SetTrigger("Attack");
        }
        // OUT OF RANGE - SEEK THE TARGET
        else
        { toSeek = true; }

        /* NOT FAST TARGET */
        if (!isTooFast(targetRB, maxTargetSpeed))
        {
            /* SITTING - Stand up and Seek the target */
            if (isSit)
            {
                isSit = false;
                _animator.SetTrigger("Stand");
            }
            toSeek = true;
        }
        /* FAST TARGET - SIT DOWN UNDER FAR CASE */
        else
        {
            Debug.Log("Too Fast!");
            // Sit down - only headtrack + orientation
            if (!isSit)
            {
                _animator.SetTrigger("Sit");
                isSit = true;
                toSeek = false;
            }
        }

        // Check for animation state
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_to") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_from"))
        {
            Debug.Log("Performing Sit Animation");
            toSeek = false;
        }

        // SEEK COMMAND VERIFIED
        if (toSeek)
        {
            currentMovement = kinematicSeek(agentRB, targetRB, maxSpeed);
            // Fix y so the agent don't fly
            currentMovement.linearVelocity.y = 0;
        }

        // Update Velocity
        agentRB.velocity = currentMovement.linearVelocity;

        // Check for movement
        if (agentRB.velocity.magnitude != 0)
        {
            _animator.SetFloat("Speed", agentRB.velocity.magnitude);
        }

        // Check for orientation changes
        if (currentMovement.rotVelocity != 0)
        {
            Quaternion deltaRot = Quaternion.Euler(new Vector3(0, currentMovement.rotVelocity * Mathf.Rad2Deg, 0));
            agentRB.MoveRotation(deltaRot);
        }
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

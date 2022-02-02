using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementOutputs;
using static AIService.aiService;
using FIMSpace.FSpine;

public class agent : MonoBehaviour
{
    public Rigidbody agentRB;
    public Rigidbody targetRB;
    public float maxSpeed;
    //public float maxRotationSpeed;
    public float idleRadius;
    public float maxTargetSpeed;
    public float jumpRadius;
    public float jumpUpForce;
    public float jumpUpAtAcceleration;
    public Vector3 targetAcceleration;
    public float attackTimeThreshold = 4f;
    public float jumpTimeThreshold = 5f;
    public float velocityMultiplier = 1f;

    private Animator _animator;
    private bool isSit = false;
    private bool toSeek = false;
    private bool lockY = true;
    private Vector3 targetLastVelocity;
    private float timeElapsedSinceLastAttack;
    private float timeElapsedSinceLastJump;


    // Test Fix Cat Animation
    private FSpineAnimator _spineAnimator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        targetLastVelocity = new Vector3(0, 0, 0);
        targetAcceleration = new Vector3(0, 0, 0);

        _spineAnimator = GetComponent<FSpineAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsedSinceLastAttack += Time.deltaTime;
        timeElapsedSinceLastJump += Time.deltaTime;

        KinematicSteeringOutput currentMovement;
        currentMovement.linearVelocity = new Vector3(0, 0, 0);
        currentMovement.rotVelocity = 0;

        calcTargetAcceleration();
        Debug.Log("Acceleration - " + targetAcceleration);

        if (targetRB.velocity.y >= jumpUpAtAcceleration)
        {
            if(timeElapsedSinceLastJump >= jumpTimeThreshold)
            {
                agentRB.AddForce(transform.up * jumpUpForce, ForceMode.Impulse);
                timeElapsedSinceLastJump = 0.0f;
            }
                
        }

        /* IDLE RANGE - AGENT MOVE NEAR TARGET */
        // Set a radius around the agent determining an idle range
        // Only head track + orientation matching, no seeking, attack the target
        if (isInRadius(agentRB.position, targetRB.position, idleRadius))
        {
            Debug.Log("In Radius - " + Time.realtimeSinceStartup % 2);
            toSeek = false;
            if (timeElapsedSinceLastAttack >= attackTimeThreshold)
            {
                timeElapsedSinceLastAttack = 0.0f;
                _animator.SetTrigger("Attack");
            }
        }
        // OUT OF RANGE - SEEK THE TARGET
        else
        { /* NOT FAST TARGET */
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
                    Debug.Log("Not Sitted!");
                    _animator.SetTrigger("Sit");
                    isSit = true;
                    toSeek = false;
                }
                else
                {
                    toSeek = false;
                }
            }

            // if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward")){
            //     Debug.Log("Jumping Forward...");
            // }
            bool isJmp = _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward");
            Debug.Log("Jumping Forward: " + isJmp);

            // Check for charge jump forward condition
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !_animator.IsInTransition(0))
            {
                // Check for charge jump animation when target is close
                if (isInRadius(agentRB.position, targetRB.position, jumpRadius) &&
                    !_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"))
                {
                    Debug.Log("Jump Forward");
                    _animator.SetTrigger("JumpForward");
                    _spineAnimator.SpineAnimatorAmount = 0;
                    Debug.Log("After Trigger state set: " + _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"));
                    maxSpeed = 6f;
                    // StartCoroutine(RecoverNormalSpeed());
                    // agentRB.AddForce(transform.up * 6f, ForceMode.Impulse);
                }
            }
        }

        // Check for animation state
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_to") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_from") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_Idle"))
        {
            Debug.Log("Performing Sit Animation");
            toSeek = false;
        }

        

        //if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"))
        //{
        //    velocityMultiplier = 2f;
        //}
        //else
        //{
        //    velocityMultiplier = 1f;
        //}

        // SEEK COMMAND VERIFIED
        if (toSeek)
        {
            currentMovement = kinematicSeek(agentRB, targetRB, maxSpeed);

            //currentMovement.linearVelocity.x *= velocityMultiplier;
            //currentMovement.linearVelocity.z *= velocityMultiplier;
        }
            
        // Update Velocity
        agentRB.velocity = new Vector3(currentMovement.linearVelocity.x, agentRB.velocity.y,
                                        currentMovement.linearVelocity.z);
        _animator.SetFloat("Speed", agentRB.velocity.magnitude);

        // Check for orientation changes
        if (currentMovement.rotVelocity != 0)
        {
            Quaternion deltaRot = Quaternion.Euler(new Vector3(0, currentMovement.rotVelocity * Mathf.Rad2Deg, 0));
            agentRB.MoveRotation(deltaRot);
        }
        // Record Target Velocity
        targetLastVelocity = targetRB.velocity;
    }

    bool isInRadius(Vector3 agent, Vector3 target, float radius)
    {
        if ((target.x - agent.x) * (target.x - agent.x) + (target.z - agent.z) * (target.z - agent.z) <= radius * radius)
            return true;

        return false;
    }

    bool isTooFast(Rigidbody target, float maxTargetSpeed)
    {
        if (target.velocity.magnitude > maxTargetSpeed)
            return true;
        return false;
    }

    void calcTargetAcceleration()
    {
        targetAcceleration = (targetRB.velocity - targetLastVelocity) / Time.fixedDeltaTime;
    }


    public void AddJumpForwardForce()
    {
        agentRB.AddForce(transform.up * 6f, ForceMode.Impulse);
    }



    [Header("Jump Forward Parameters")]
    public float stage1 = 5f;
    public float stage2 = 8f;
    public float stage3 = 12f;
    public float stage4 = 7f;
    public float stage5 = 2.5f;


    public void CatJumpForward_1()
    {
        maxSpeed = stage1;
    }

    public void CatJumpForward_2()
    {
        maxSpeed = stage2;
    }

    public void CatJumpForward_3()
    {
        maxSpeed = stage3;
    }

    public void CatJumpForward_4()
    {
        maxSpeed = stage4;
    }

    public void CatJumpForward_5()
    {
        maxSpeed = stage5;
    }
}

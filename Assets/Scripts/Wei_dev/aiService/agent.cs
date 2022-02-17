using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementOutputs;
using static AIService.KinematicSeek;
using AIService;

using FIMSpace.FSpine;

public class agent : MonoBehaviour
{
    public Rigidbody agentRB;
    public Rigidbody targetRB;

    public float maxSpeed;
    public float maxTargetSpeed;
    public float maxAcceleration;

    public float idleRadius;
    public float jumpRadius;
    public float jumpUpForce;
    public float jumpUpAtAcceleration;
    
    public float attackTimeThreshold = 4f;
    public float jumpTimeThreshold = 5f;
    public float velocityMultiplier = 1f;

    public float maxAngularAcceleration;
    public float maxRotation;

    public float targetRadius;
    public float slowRadius;
    public float timeToTarget;

    public Vector3 targetAcceleration;

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

        //KinematicSteeringOutput currentMovement;
        //currentMovement.linearVelocity = new Vector3(0, 0, 0);
        //currentMovement.rotVelocity = 0;

        DynamicSteeringOutput currentMovement;
        currentMovement.linearAccel = new Vector3(0, 0, 0);
        currentMovement.rotAccel = 0;

        calcTargetAcceleration();
        Debug.Log("Acceleration - " + targetAcceleration);

        // Animation Listeners
        JumpUpListener();
        AttackListener();
        FastTargetListener();
        ChargeJumpListener();

        // Disable movement while sitting animation is in progress
        NoSeekWhileSit();

        // SEEK COMMAND VERIFIED
        if (toSeek)
        {
            //currentMovement = kinematicSeek(agentRB, targetRB, maxSpeed);
            DynamicArrive dynamicArrive = new DynamicArrive(agentRB, targetRB, maxAcceleration, maxSpeed, targetRadius, slowRadius);
            DynamicFace dynamicFace = new DynamicFace(agentRB, targetRB, maxAngularAcceleration, maxRotation, targetRadius, slowRadius);
            currentMovement = dynamicArrive.getSteering();
            currentMovement.rotAccel = dynamicFace.getSteering().rotAccel;
            
        }
        
        UpdateRigidBody(currentMovement);
    }

    void UpdateRigidBody(KinematicSteeringOutput currentMovement)
    {
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

    void UpdateRigidBody(DynamicSteeringOutput i_steering)
    {
        // Update position and orientation
        agentRB.position += agentRB.velocity * Time.deltaTime;
        agentRB.rotation = Quaternion.Euler(new Vector3(0, (agentRB.angularVelocity.y * Time.deltaTime), 0));

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

    void NoSeekWhileSit()
    {
        // Check for animation state
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_to") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_from") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Sit_Idle"))
        {
            Debug.Log("Performing Sit Animation");
            toSeek = false;
        }
    }

    void JumpUpListener()
    {
        timeElapsedSinceLastJump += Time.deltaTime;

        if (targetRB.velocity.y >= jumpUpAtAcceleration)
        {
            if (timeElapsedSinceLastJump >= jumpTimeThreshold)
            {
                // TempGameManager.Instance.OnCatJumpUp();
                _animator.SetTrigger("JumpUp");

                // agentRB.AddForce(transform.up * jumpUpForce, ForceMode.Impulse);
                timeElapsedSinceLastJump = 0.0f;
            }

        }
    }

    void AttackListener()
    {
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
                // TempGameManager.Instance.OnCatAttack();
                _animator.SetTrigger("Attack");
            }
        }
    }

    void FastTargetListener()
    {
        /* NOT FAST TARGET */
        if (!isTooFast(targetRB, maxTargetSpeed))
        {
            /* SITTING - Stand up and Seek the target */
            if (isSit)
            {
                isSit = false;
                // TempGameManager.Instance.OnCatStand();
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
                // TempGameManager.Instance.OnCatSit();
                _animator.SetTrigger("Sit");
                isSit = true;
                toSeek = false;
            }
            else
            {
                toSeek = false;
            }
        }
    }

    void ChargeJumpListener()
    {
        // Check for charge jump forward condition
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !_animator.IsInTransition(0))
        {
            // Check for charge jump animation when target is close
            if (isInRadius(agentRB.position, targetRB.position, jumpRadius) &&
                !_animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"))
            {
                Debug.Log("Jump Forward");
                // TempGameManager.Instance.OnCatJumpForward();
                _animator.SetTrigger("JumpForward");
                Debug.Log("After Trigger state set: " + _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"));
                maxSpeed = 6f;
                // StartCoroutine(RecoverNormalSpeed());
                // agentRB.AddForce(transform.up * 6f, ForceMode.Impulse);
            }
        }
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



    #region TEMP EVENTS

    /// <summary>
    /// Control the movement speed to tween the animation effect
    /// </summary>
    [Header("Jump Forward Parameters")]
    public float jumpforward_stage1 = 5f;
    public float jumpforward_stage2 = 8f;
    public float jumpforward_stage3 = 12f;
    public float jumpforward_stage4 = 7f;
    public float jumpforward_stage5 = 2.5f;

    /// <summary>
    /// The default spineAnmatorAmount value that should be applied to the cat at the end of each animation
    /// </summary>
    [Header("Spine Animator Parameters")]
    public float spineAnimatorAmount = 0.2f;

    /* ANIMATION EVENTS - JUMPFORWARD */
    // The Followings are the animation event that will set different attribute of cats at different animation frames. 
    // It is for temp pitch use and should be refactor later

    public void CatJumpForward_1()
    {
        maxSpeed = jumpforward_stage1;
    }

    public void CatJumpForward_2()
    {
        maxSpeed = jumpforward_stage2;
    }

    public void CatJumpForward_3()
    {
        maxSpeed = jumpforward_stage3;
    }

    public void CatJumpForward_4()
    {
        maxSpeed = jumpforward_stage4;
    }

    public void CatJumpForward_5()
    {
        maxSpeed = jumpforward_stage5;
    }


    /// <summary>
    /// Control the movement velocity to get better physics (Up Velocity)
    /// </summary>
    [Header("Jump Up Parameters")]
    public float jumpup_stage1 = 5f;
    public float jumpup_stage2 = 8f;
    public float jumpup_stage3 = 12f;
    public float jumpup_stage4 = 12f;
    public float jumpup_stage5 = 12f;


    /* ANIMATION EVENTS - JUMPUP */
    // The Followings are the animation event that will set different attribute of cats at different animation frames. 
    // It is for temp pitch use and should be refactor later

    public void CatJumpUp_1()
    {
        SetCatJumpUp(jumpup_stage1);
        maxSpeed = 6f;
    }

    public void CatJumpUp_2()
    {
        SetCatJumpUp(jumpup_stage2);
    }

    public void CatJumpUp_3()
    {
        SetCatJumpUp(jumpup_stage3);
    }

    public void CatJumpUp_4()
    {
        SetCatJumpUp(jumpup_stage4);
    }

    public void CatJumpUp_5()
    {
        SetCatJumpUp(jumpup_stage5);
        maxSpeed = 2.5f;
    }

    void SetCatJumpUp(float stage)
    {
        var oldVelocity = agentRB.velocity;
        agentRB.velocity = new Vector3(oldVelocity.x, stage, oldVelocity.z);
    }

    /* GAMEMANAGER EVENTS*/
    // The TempGameManager is a class that is incharge of UI & Audio, and should be refactored by optimized structure


    public void PlaySFXAttack()
    {
        // TempGameManager.Instance.PlaySFXAttack();
    }

    public void PlaySFXJump()
    {
        // TempGameManager.Instance.PlaySFXJump();
    }

    public void PlayRandomCatAudio()
    {
        // TempGameManager.Instance.PlayRandomCatAudio();
    }

    #endregion TEMP EVENTS

}

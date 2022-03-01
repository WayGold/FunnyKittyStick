using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MovementOutputs;
using static AIService.KinematicSeek;
using AIService;

using FIMSpace.FSpine;

public class agent : MonoBehaviour
{
    [SerializeField] public List<JumpToData> JumpToList;
    [SerializeField] public List<GameObject> allTriggers;

    public GameObject StickFish;

    public Rigidbody agentRB;
    public Rigidbody targetRB;
    public Rigidbody agentCoordAux;
    public Rigidbody auxRB;

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
    private bool isThrowing = false;
    private GameObject throwToTarget = null;
    private bool lockY = true;
    private Vector3 targetLastVelocity;
    private float timeElapsedSinceLastAttack;
    private float timeElapsedSinceLastJump;

    [SerializeField] private Vector3 lastVelocity;


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

        //calcTargetAcceleration();
        //Debug.Log("Acceleration - " + targetAcceleration);

        // Animation Listeners
        JumpUpListener();
        AttackListener();
        FastTargetListener();
        //ChargeJumpListener();

        // Check for throwing to preset jumpTo locations
        //ThrowListener();
        ThrowArrivalUpdate();

        // Disable movement while sitting animation is in progress
        NoSeekWhileSit();

        // SEEK COMMAND VERIFIED
        if (toSeek)
        {
            //currentMovement = kinematicSeek(agentRB, targetRB, maxSpeed);
            DynamicArrive dynamicArrive = new DynamicArrive(agentRB, targetRB, maxAcceleration, maxSpeed, targetRadius, slowRadius);
            currentMovement = dynamicArrive.getSteering();

            targetRadius = 0.05f;
            slowRadius = 0.15f;

            //DynamicFace dynamicFace = new DynamicFace(agentRB, targetRB, auxRB, maxAngularAcceleration, maxRotation, targetRadius, slowRadius);
            //currentMovement.rotAccel = dynamicFace.getSteering().rotAccel;

            DynamicLWYAG dynamicLWYAG = new DynamicLWYAG(agentRB, auxRB, maxAngularAcceleration, maxRotation, 0.05f, 0.15f);
            currentMovement.rotAccel = dynamicLWYAG.getSteering().rotAccel;

            currentMovement.linearAccel.y = 0;
        }

        // No Rotation While Falling
        if (agentRB.velocity.y < -0.2 || agentRB.velocity.y > 0.2)
        {
            SetSpineAnimationAmount(0);
            agentRB.freezeRotation = true;
            currentMovement.linearAccel = new Vector3(0, 0, 0);
            currentMovement.rotAccel = 0;
        }
        else
        {
            SetSpineAnimationAmount(100);
            agentRB.freezeRotation = false;
        }

        if (!isThrowing)
            UpdateRigidBody(currentMovement);
    }

    #region JUMP THROW WITH COLLISON TRIGGER AREAS
    private void OnTriggerStay(Collider other)
    {
        if (!isThrowing)
        {
            foreach (var triggerBox in allTriggers)
            {
                if (triggerBox.GetComponent<Collider>() == other)
                {
                    if (other.tag == "bed")
                    {
                        Debug.Log("In Bed Trigger Box!");
                        // Check for our fish position against jumpTo target,
                        // within a range and higher than the jumpTo target
                        if (Vector3.Distance(new Vector3(targetRB.position.x, 0, targetRB.position.z),
                                            new Vector3(JumpToList[0].JumpPointObj.transform.position.x, 0,
                                            JumpToList[0].JumpPointObj.transform.position.z)) <= 4 &&
                                            targetRB.position.y > JumpToList[0].JumpPointObj.transform.position.y)
                        {
                            Debug.Log("Throw to Bed!");
                            // Throw to Target
                            ThrowToTarget(JumpToList[0].JumpPointObj, JumpToList[0].AddHeight);
                        }
                    }

                    if (other.tag == "chair")
                    {
                        Debug.Log("In Chair Trigger Box!");
                        if (Vector3.Distance(new Vector3(targetRB.position.x, 0, targetRB.position.z),
                                            new Vector3(JumpToList[1].JumpPointObj.transform.position.x, 0,
                                            JumpToList[1].JumpPointObj.transform.position.z)) <= 4 &&
                                            targetRB.position.y > JumpToList[1].JumpPointObj.transform.position.y)
                        {
                            Debug.Log("Throw to Chair!");
                            // Throw to Target
                            ThrowToTarget(JumpToList[1].JumpPointObj, JumpToList[1].AddHeight);
                        }
                    }

                    if (other.tag == "desk")
                    {
                        Debug.Log("In Desk Trigger Box!");
                        if (Vector3.Distance(new Vector3(targetRB.position.x, 0, targetRB.position.z),
                                            new Vector3(JumpToList[3].JumpPointObj.transform.position.x, 0,
                                            JumpToList[3].JumpPointObj.transform.position.z)) <= 4 &&
                                            targetRB.position.y > JumpToList[3].JumpPointObj.transform.position.y)
                        {
                            Debug.Log("Throw to Desk!");
                            // Throw to Target
                            ThrowToTarget(JumpToList[3].JumpPointObj, JumpToList[3].AddHeight);
                        }
                    }

                    if (other.tag == "closet")
                    {
                        Debug.Log("In Closet Trigger Box!");
                        if (Vector3.Distance(new Vector3(targetRB.position.x, 0, targetRB.position.z),
                                            new Vector3(JumpToList[2].JumpPointObj.transform.position.x, 0,
                                            JumpToList[2].JumpPointObj.transform.position.z)) <= 4 &&
                                            targetRB.position.y > JumpToList[2].JumpPointObj.transform.position.y)
                        {
                            Debug.Log("Throw to Closet!");
                            // Throw to Target
                            ThrowToTarget(JumpToList[2].JumpPointObj, JumpToList[2].AddHeight);
                        }
                    }
                }
            }
        }
    }

    void ThrowToTarget(GameObject target, float addHeight)
    {
        Vector3 result = ProjectionThrow.CaculateThrowVelocity(agentRB.gameObject,
                            target.transform.position, addHeight);

        foreach (var collider in gameObject.GetComponents<BoxCollider>())
            collider.enabled = false;

        // Disable Seeking while Throwing, turn on isThrowing flag
        toSeek = false;
        isThrowing = true;
        throwToTarget = target;
        Debug.Log("Throwing with init velocity: " + result);
        _animator.SetTrigger("JumpForward");
        agentRB.velocity = result;
    }

    void ThrowArrivalUpdate()
    {
        if (isThrowing)
        {
            Debug.Log("Currently Throwing, Check for arrival...");
            // If the agent arrive at the target
            if ((agentRB.position.x >= throwToTarget.transform.position.x - 1f && agentRB.position.x <= throwToTarget.transform.position.x + 1f) &&
                (agentRB.position.z >= throwToTarget.transform.position.z - 1f && agentRB.position.z <= throwToTarget.transform.position.z + 1f))
            {
                foreach (var collider in gameObject.GetComponents<BoxCollider>())
                    collider.enabled = true;

                //agentRB.velocity = Vector3.zero;
                agentRB.velocity = agentRB.velocity / 2;
                // Reset isThrowing flag and target aux
                isThrowing = false;
                throwToTarget = null;
                // Enable Seek
                toSeek = true;
                Debug.Log("Throw Arrived...");

            }
            else
            {
                toSeek = false;
                isThrowing = true;
                Debug.Log("Hasn't Arrived, Throw in Progress...");
            }
        }
    }
    #endregion

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
        agentRB.rotation = Quaternion.Euler(new Vector3(0, agentRB.rotation.eulerAngles.y + (agentRB.angularVelocity.y * Time.deltaTime) * Mathf.Rad2Deg, 0));
        //Debug.Log(lastVelocity);
        // Update velocity and rotation.
        agentRB.velocity += i_steering.linearAccel * Time.deltaTime;
        agentRB.angularVelocity += new Vector3(0, -i_steering.rotAccel * Time.deltaTime, 0);
        _animator.SetFloat("Speed", agentRB.velocity.magnitude);

        lastVelocity = agentRB.velocity;

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

    #region ANIMATION LISTENERS

    void ThrowListener()
    {
        // Already triggerred to jump, Check for Arrival here
        if (isThrowing)
        {
            // If the agent arrive at the target
            if ((agentRB.position.x >= throwToTarget.transform.position.x - 1f && agentRB.position.x <= throwToTarget.transform.position.x + 1f) &&
                (agentRB.position.z >= throwToTarget.transform.position.z - 1f && agentRB.position.z <= throwToTarget.transform.position.z + 1f))
            {
                foreach (var collider in gameObject.GetComponents<BoxCollider>())
                    collider.enabled = true;

                //agentRB.velocity = Vector3.zero;
                agentRB.velocity = agentRB.velocity / 2;
                // Reset isThrowing flag and target aux
                isThrowing = false;
                throwToTarget = null;
                // Enable Seek
                toSeek = true;
                Debug.Log("Throw Arrived...");

            }
            else
            {
                toSeek = false;
                isThrowing = true;
                Debug.Log("Throw in Progress...");
            }
        }
        // Else Check for all target to jump to
        else
        {
            foreach (var jumpTarget in JumpToList)
            {
                // If the agent enter a range and there is a meaningful height difference
                if (Vector3.Distance(new Vector3(jumpTarget.JumpPointObj.transform.position.x, 0, jumpTarget.JumpPointObj.transform.position.z),
                    new Vector3(agentRB.position.x, 0, agentRB.position.z)) <= jumpTarget.DetectRange &&
                    Mathf.Abs(jumpTarget.JumpPointObj.transform.position.y - agentRB.position.y) > 3 &&
                    Vector3.Distance(jumpTarget.JumpPointObj.transform.position, StickFish.transform.position) <= 2)
                {
                    Debug.Log("In Range of: " + jumpTarget.JumpPointObj.name);
                    // And if the agent is at a lower level
                    if (agentRB.position.y < jumpTarget.JumpPointObj.transform.position.y)
                    {
                        Vector3 result = ProjectionThrow.CaculateThrowVelocity(agentRB.gameObject, jumpTarget.JumpPointObj.transform.position, jumpTarget.AddHeight);

                        //float timeToJumpToTarget = 1.0f;
                        // This was originally used for calc acceleration
                        //result = result / timeToJumpToTarget;

                        foreach (var collider in gameObject.GetComponents<BoxCollider>())
                            collider.enabled = false;

                        // Disable Seeking while Throwing, turn on isThrowing flag
                        toSeek = false;
                        isThrowing = true;
                        throwToTarget = jumpTarget.JumpPointObj;
                        Debug.Log("Throwing with init velocity: " + result);
                        agentRB.velocity = result;
                    }
                }
            }
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
                //_animator.SetTrigger("JumpUp");

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
            //Debug.Log("In Radius - " + Time.realtimeSinceStartup % 2);
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
                SetSpineAnimationAmount(0);
                _animator.SetTrigger("JumpForward");
                Debug.Log("After Trigger state set: " + _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"));
                maxSpeed = 6f;
                // StartCoroutine(RecoverNormalSpeed());
                // agentRB.AddForce(transform.up * 6f, ForceMode.Impulse);
            }
        }
    }

    #endregion

    public void SetSpineAnimationAmount(float amount)
    {
        _spineAnimator.SpineAnimatorAmount = amount;
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
        targetAcceleration = (targetRB.velocity - targetLastVelocity) / Time.deltaTime;
    }


    public void AddJumpForwardForce()
    {
        agentRB.AddForce(transform.up * 6f, ForceMode.Impulse);
    }



    //#region TEMP EVENTS

    ///// <summary>
    ///// Control the movement speed to tween the animation effect
    ///// </summary>
    //[Header("Jump Forward Parameters")]
    //public float jumpforward_stage1 = 5f;
    //public float jumpforward_stage2 = 8f;
    //public float jumpforward_stage3 = 12f;
    //public float jumpforward_stage4 = 7f;
    //public float jumpforward_stage5 = 2.5f;

    ///// <summary>
    ///// The default spineAnmatorAmount value that should be applied to the cat at the end of each animation
    ///// </summary>
    //[Header("Spine Animator Parameters")]
    //public float spineAnimatorAmount = 0.2f;

    ///* ANIMATION EVENTS - JUMPFORWARD */
    //// The Followings are the animation event that will set different attribute of cats at different animation frames. 
    //// It is for temp pitch use and should be refactor later

    //public void CatJumpForward_1()
    //{
    //    maxSpeed = jumpforward_stage1;
    //}

    //public void CatJumpForward_2()
    //{
    //    maxSpeed = jumpforward_stage2;
    //}

    //public void CatJumpForward_3()
    //{
    //    maxSpeed = jumpforward_stage3;
    //}

    //public void CatJumpForward_4()
    //{
    //    maxSpeed = jumpforward_stage4;
    //}

    //public void CatJumpForward_5()
    //{
    //    maxSpeed = jumpforward_stage5;
    //}


    ///// <summary>
    ///// Control the movement velocity to get better physics (Up Velocity)
    ///// </summary>
    //[Header("Jump Up Parameters")]
    //public float jumpup_stage1 = 5f;
    //public float jumpup_stage2 = 8f;
    //public float jumpup_stage3 = 12f;
    //public float jumpup_stage4 = 12f;
    //public float jumpup_stage5 = 12f;


    ///* ANIMATION EVENTS - JUMPUP */
    //// The Followings are the animation event that will set different attribute of cats at different animation frames. 
    //// It is for temp pitch use and should be refactor later

    //public void CatJumpUp_1()
    //{
    //    SetCatJumpUp(jumpup_stage1);
    //    maxSpeed = 6f;
    //}

    //public void CatJumpUp_2()
    //{
    //    SetCatJumpUp(jumpup_stage2);
    //}

    //public void CatJumpUp_3()
    //{
    //    SetCatJumpUp(jumpup_stage3);
    //}

    //public void CatJumpUp_4()
    //{
    //    SetCatJumpUp(jumpup_stage4);
    //}

    //public void CatJumpUp_5()
    //{
    //    SetCatJumpUp(jumpup_stage5);
    //    maxSpeed = 2.5f;
    //}

    //void SetCatJumpUp(float stage)
    //{
    //    var oldVelocity = agentRB.velocity;
    //    agentRB.velocity = new Vector3(oldVelocity.x, stage, oldVelocity.z);
    //}

    ///* GAMEMANAGER EVENTS*/
    //// The TempGameManager is a class that is incharge of UI & Audio, and should be refactored by optimized structure


    //public void PlaySFXAttack()
    //{
    //    // TempGameManager.Instance.PlaySFXAttack();
    //}

    //public void PlaySFXJump()
    //{
    //    // TempGameManager.Instance.PlaySFXJump();
    //}

    //public void PlayRandomCatAudio()
    //{
    //    // TempGameManager.Instance.PlayRandomCatAudio();
    //}

    //#endregion TEMP EVENTS

}

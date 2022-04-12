using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MovementOutputs;
using static AIService.KinematicSeek;
using AIService;

using FIMSpace.FSpine;

public class agent : MonoBehaviour
{
    [SerializeField] public List<Rigidbody> jumpStartPoints;
    [SerializeField] public List<GameObject> jumpTargets;

    public RuntimeAnimatorController catAnimatior;
    public RuntimeAnimatorController jumpAnimator;

    public Rigidbody stickFish;

    [SerializeField] private Rigidbody jumpStartPoint;
    [SerializeField] private GameObject jumpTarget;
    private float startPointExtent;
    private float TargetPointExtent;
    private int jumpAnimationIndex=0;

    public Rigidbody agentRB;
    public Rigidbody targetRB;
    public Rigidbody robotRB;
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
    [SerializeField] private bool isSit = false;
    [SerializeField] private bool toSeek = false;
    [SerializeField] private bool shouldJump = false;
    private bool lockY = true;
    [SerializeField] private bool sitRobot = false;
    [SerializeField] private bool canMove = true;

    private GameObject throwToTarget = null;

    private Vector3 targetLastVelocity;

    private float timeElapsedSinceLastAttack;
    private float timeElapsedSinceLastJump;

    [SerializeField] private Vector3 lastVelocity;

    // Test Fix Cat Animation
    private FSpineAnimator _spineAnimator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        targetLastVelocity = new Vector3(0, 0, 0);
        targetAcceleration = new Vector3(0, 0, 0);

        _spineAnimator = GetComponent<FSpineAnimator>();

        if(_animator.runtimeAnimatorController != catAnimatior)
            _animator.runtimeAnimatorController = catAnimatior;

    }

    // Update is called once per frame
    void Update()
    {
        DynamicSteeringOutput currentMovement;
        currentMovement.linearAccel = new Vector3(0, 0, 0);
        currentMovement.rotAccel = 0;

        //calcTargetAcceleration();
        //Debug.Log("Acceleration - " + targetAcceleration);

        // Animation Listeners
        if(canMove)
        {
            JumpUpListener();
            AttackListener();
            FastTargetListener();
        }
        //ChargeJumpListener();

        // Disable movement while sitting animation is in progress
        NoSeekWhileSit();

        // Check for throwing to preset jumpTo locations
        JumpListener();

        // SEEK COMMAND VERIFIED
        if (toSeek)
        {
            if (shouldJump)
            {
                DynamicArrive dynamicArrive = new DynamicArrive(agentRB, jumpStartPoint, maxAcceleration, maxSpeed, targetRadius, slowRadius);
                currentMovement = dynamicArrive.getSteering();

                targetRadius = 0.05f;
                slowRadius = 0.15f;

                DynamicLWYAG dynamicLWYAG = new DynamicLWYAG(agentRB, jumpStartPoint, maxAngularAcceleration, maxRotation, 0.05f, 0.15f);
                currentMovement.rotAccel = dynamicLWYAG.getSteering().rotAccel;

                currentMovement.linearAccel.y = 0;
            }
            else if(sitRobot)
            {
                //change head track to robot
            }
            else
                {
                DynamicArrive dynamicArrive = new DynamicArrive(agentRB, targetRB, maxAcceleration, maxSpeed, targetRadius, slowRadius);
                currentMovement = dynamicArrive.getSteering();

                targetRadius = 0.05f;
                slowRadius = 0.15f;

                DynamicLWYAG dynamicLWYAG = new DynamicLWYAG(agentRB, auxRB, maxAngularAcceleration, maxRotation, 0.05f, 0.15f);
                currentMovement.rotAccel = dynamicLWYAG.getSteering().rotAccel;

                currentMovement.linearAccel.y = 0;
            }
        }

        // No Rotation While Falling
        NoRotationWhileFalling(currentMovement);

        UpdateRigidBody(currentMovement);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "bedbotton")//bed botton, cat will got shocked
        {
            canMove = false;
            toSeek = false;
            _animator.SetTrigger("isShocked");
        }
        if(other.tag=="laptop")
        {
            canMove = false;
            toSeek = false;
            isSit = true;
            gameObject.transform.LookAt(other.transform);
            _animator.SetTrigger("Sit");
            Invoke("LaptopStand", 10);
            agentRB.GetComponent<HeadTrackingDebug>().target = GameObject.Find("laptopscreen");
            agentRB.GetComponent<HeadTrackingDebug>().TrackTarget();
            other.gameObject.GetComponentInChildren<RawImage>().enabled = true;
        }
    }
    void LaptopStand()
    {
        GameObject LaptopDetectionArea = GameObject.Find("LaptopDetectionArea");
        if(LaptopDetectionArea!=null)
        {
            targetRB = stickFish;
            agentRB.GetComponent<HeadTrackingDebug>().target = stickFish.gameObject;
            agentRB.GetComponent<HeadTrackingDebug>().TrackTarget();
            GameObject.Destroy(GameObject.Find("LaptopDetectionArea"));
            canMove = true;
            toSeek = true;
            isSit = false;
            _animator.SetTrigger("Stand");
        }
    }

    #region JUMP THROW WITH COLLISON TRIGGER AREAS
    bool couldDetect = true;
    private void OnTriggerStay(Collider other)
    {
        if (!couldDetect) return;
        if (shouldJump) return;
        switch (other.tag)
        {
            case "chair":
                {
                    shouldJump = true;
                    if (other.gameObject.name == "ChairJumpDetectionArea1")
                    {
                        jumpStartPoint = jumpStartPoints[0];
                        jumpAnimationIndex = 0;
                    }
                    else if (other.gameObject.name == "ChairJumpDetectionArea2")
                    {
                        jumpStartPoint = jumpStartPoints[1];
                        jumpAnimationIndex = 1;
                    }
                    jumpTarget = jumpTargets[0];
                    startPointExtent = 1f;
                    TargetPointExtent = 5f;
                    break;
                }
            case "desk":
                {
                    shouldJump = true;
                    if (other.gameObject.name == "DeskJumpDetectionArea1")
                    {
                        jumpStartPoint = jumpStartPoints[2];
                        jumpAnimationIndex = 2;
                        jumpTarget = jumpTargets[1];
                    }
                    else if (other.gameObject.name == "DeskJumpDetectionArea2")
                    {
                        jumpStartPoint = jumpStartPoints[7];
                        jumpAnimationIndex =7;
                        jumpTarget = jumpTargets[5];
                    }
                    startPointExtent = 1f;
                    TargetPointExtent = 5f;
                    break;
                }
            case "bed":
                {
                    shouldJump = true;
                    if (other.gameObject.name == "BedJumpDetectionArea1")
                    {
                        jumpStartPoint = jumpStartPoints[3];
                        jumpAnimationIndex = 3;
                    }
                    else if (other.gameObject.name == "BedJumpDetectionArea2")
                    {
                        jumpStartPoint = jumpStartPoints[4];
                        jumpAnimationIndex = 4;
                    }
                    jumpTarget = jumpTargets[2];
                    startPointExtent = 1f;
                    TargetPointExtent = 5f;
                    break;
                }
            case "closet":
                {
                    shouldJump = true;
                    jumpStartPoint = jumpStartPoints[5];
                    jumpAnimationIndex = 5;
                    jumpTarget = jumpTargets[3];
                    startPointExtent = 1f;
                    TargetPointExtent = 5f;
                    break;
                }
            case "robot":
                {
                    if (isSit == true) return;
                    shouldJump = true;
                    
                    jumpStartPoint = jumpStartPoints[6];
                    jumpAnimationIndex = 6;
                    jumpTarget = jumpTargets[4];
                    startPointExtent = 0.5f;
                    TargetPointExtent = 3f;
                    break;
                }
        }
    }

    void JumpListener()
    {
        if (shouldJump)
        {
            //print("Distance(targetRB.transform.position, jumpTarget.transform.position):" + Vector3.Distance(targetRB.transform.position, jumpTarget.transform.position));
            //print("Distance(agentRB.transform.position, jumpStartPoint.transform.position):" + Vector3.Distance(agentRB.transform.position, jumpStartPoint.transform.position));
            if (Mathf.Abs(targetRB.transform.position.x-jumpTarget.transform.position.x) < TargetPointExtent &&
                Mathf.Abs(targetRB.transform.position.z - jumpTarget.transform.position.z) < TargetPointExtent)
            {
                if (Vector3.Distance(agentRB.transform.position, jumpStartPoint.transform.position) < startPointExtent)
                {
                    agentRB.GetComponent<FSpineAnimator>().enabled = false;
                    agentRB.transform.LookAt(new Vector3( targetRB.transform.position.x,agentRB.transform.position.y, targetRB.transform.position.z));
                    _animator.runtimeAnimatorController = jumpAnimator;
                    _animator.SetInteger("JumpIndex", jumpAnimationIndex);
                    couldDetect = false;
                }
            }
            else
            {
                shouldJump = false;
            }
        }
    }
    void JumpArrivalUpdate()
    {
        _animator.runtimeAnimatorController = catAnimatior;
        gameObject.transform.position = jumpTarget.transform.position;
        agentRB.GetComponent<FSpineAnimator>().enabled = true;
        shouldJump = false;
        couldDetect = true;
    }
    #region JumpAnimationEvent
    public void ChairJumpOver_1()
    {
        JumpArrivalUpdate();
    }

    public void ChairJumpOver_2()
    {
        JumpArrivalUpdate();
    }
    public void DeskJumpOver_1()
    {
        JumpArrivalUpdate();
    }
    public void DeskJumpStart_2()
    {
        GameObject.Find("DeskJumpDetectionArea2").GetComponent<JumpArea>().DeActiveBoxCollider();
    }
    public void DeskJumpOver_2()
    {
        JumpArrivalUpdate();

        //exceptional case: set the cat on the ground
        gameObject.transform.position = new Vector3(20, 0, 8.5f);
        //only jump once, so destroy the detection area
        //Destroy(GameObject.Find("DeskJumpDetectionArea2"));
    }
    public void BedJumpOver_1()
    {
        JumpArrivalUpdate();
    }
    public void BedJumpOver_2()
    {
        JumpArrivalUpdate();
    }
    public void ClosetJumpOver()
    {
        JumpArrivalUpdate();
    }

    public void RobotJumpOver()
    {
        _animator.runtimeAnimatorController = catAnimatior;
        gameObject.transform.position = jumpTarget.transform.position;

        sitRobot = true;
        shouldJump = false;
        couldDetect = true;
        toSeek = false;
        canMove = false;
        isSit = true;

        _animator.SetTrigger("Sit");
        _animator.Play("Cat|Sit_to");
        agentRB.transform.SetParent(robotRB.transform.parent);
        agentRB.transform.localPosition = Vector3.zero;

        robotRB.GetComponentInParent<Robot>().StartRobotPower(agentRB.gameObject);

        agentRB.GetComponent<FSpineAnimator>().enabled = false;
        agentRB.GetComponent<HeadTrackingDebug>().target = robotRB.gameObject;
        agentRB.GetComponent<HeadTrackingDebug>().TrackTarget();
    }

    public void RobotBreak()
    {
        sitRobot = false;
        isSit = false;
        toSeek = true;
        canMove = true;
        _animator.SetTrigger("isShocked");

        targetRB = stickFish;
        agentRB.GetComponent<FSpineAnimator>().enabled = true;
        agentRB.GetComponent<HeadTrackingDebug>().target = stickFish.gameObject;
        agentRB.GetComponent<HeadTrackingDebug>().TrackTarget();
    }

    public void BedBottonOver()
    {
        toSeek = true;
        canMove = true;
        Destroy(GameObject.Find("BedBottonDetectionArea"));
    }
    #endregion
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
            agentRB.angularVelocity = new Vector3(agentRB.angularVelocity.x, maxRotation, agentRB.angularVelocity.z);
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

    void NoRotationWhileFalling(DynamicSteeringOutput currentMovement)
    {
        if (Mathf.Abs( agentRB.velocity.y) > 2f)
        {
            SetSpineAnimationAmount(0);
            agentRB.freezeRotation = true;
            currentMovement.linearAccel = new Vector3(0, 0, 0);
            currentMovement.rotAccel = 0;
            _animator.SetBool("IsFalling", true);
        }
        else
        {
            SetSpineAnimationAmount(100);
            agentRB.freezeRotation = false;
            _animator.SetBool("IsFalling", false);
        }
    }
    void JumpUpListener()
    {
        timeElapsedSinceLastJump += Time.deltaTime;

        if (targetRB.velocity.y >= jumpUpAtAcceleration)
        {
            if (timeElapsedSinceLastJump >= jumpTimeThreshold)
            {
                print("targetRB.velocity.y:" + targetRB.velocity.y);
                agentRB.AddForce(transform.up * jumpUpForce, ForceMode.Impulse);
                timeElapsedSinceLastJump = 0.0f;
            }
        }
    }

    void AttackListener()
    {
        timeElapsedSinceLastAttack += Time.deltaTime;
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
                _animator.SetTrigger("Stand");
            }
            if(!toSeek)
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
                if(toSeek)
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
                SetSpineAnimationAmount(0);
                _animator.SetTrigger("JumpForward");
                Debug.Log("After Trigger state set: " + _animator.GetCurrentAnimatorStateInfo(0).IsName("Cat|Jump_Forward"));
                maxSpeed = 5f;
                //StartCoroutine(RecoverNormalSpeed());
                agentRB.AddForce(transform.up * 6f, ForceMode.Impulse);
            }
        }
    }

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

    #region TEMP EVENTS

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

    #endregion TEMP EVENTS

}

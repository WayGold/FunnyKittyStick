using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontLegsTracking : MonoBehaviour
{
    #region Variables.

    public Transform leftIK;
    public Transform leftPole;
    public Transform rightIK;
    public Transform rightPole;
    public Transform target;

    public float maxDistance;

    private Vector3 _leftHomePos;
    private Vector3 _rightHomePos;
    private bool _moving;

    #endregion

    #region Methods.

    private int CheckDistance()
    {
        float leftDist = Vector3.Distance(leftIK.position, target.position);
        float rightDist = Vector3.Distance(rightIK.position, target.position);

        if (leftDist >= rightDist)
        {
            if (leftDist >= maxDistance)
            {
                // Cannot reach.
                return 2;
            }

            // Use left leg.
            return 0;
        }

        if (rightDist >= maxDistance)
        {
            // Cannot reach;
            return 2;
        }

        // Use right leg.
        return 1;
    }

    private void MoveLeg(Transform leg, Transform target, float time)
    {
        Vector3 startPos = leg.position;
        leg.position = Vector3.Lerp(startPos, target.position, time);
    }

    private void MoveLeg(Transform leg, Vector3 targetPos, float time)
    {
        Vector3 startPos = leg.position;
        leg.position = Vector3.Lerp(startPos, targetPos, time);
    }

    /*private IEnumerator LegTracking()
    {
        _moving = true;

        float timeElapsed = 0;

        /*do
        {
            
        } 
        while ();#1#
    }*/

    private void LegsTracking()
    {
        int check = CheckDistance();

        if (check == 0)
        {
            //MoveLeg(rightIK, _rightHomePos);
            //MoveLeg(leftIK, target);
        }
        else if (check == 1)
        {
            //MoveLeg(leftIK, _leftHomePos);
            //MoveLeg(rightIK, target);
        }
        else
        {
            //MoveLeg(leftIK, _leftHomePos);
            //MoveLeg(rightIK, _rightHomePos);
        }
    }

#endregion

    #region Life-cycle Callbacks.

    private void Start()
    {
        _leftHomePos = leftIK.position;
        _rightHomePos = rightIK.position;
    }

    private void Update()
    {
        LegsTracking();
    }

    #endregion
}

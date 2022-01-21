using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HeadTracking : MonoBehaviour
{
    #region Variables.
    
    public int type;  // Head tracking animation type.

    public MultiAimConstraint aimConstraint;  // Unity's aim constraint component.
    
    // For custom head tracking.
    public Transform target;
    public Transform headBone;
    public float maxAngle;
    public float trackingSpeed;

    #endregion

    #region Methods.

    private void CustomHeadTracking()
    {
        Quaternion currentLocalRotation = headBone.localRotation;
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle constraint.
        targetLocalLookDir = Vector3.RotateTowards(
            Vector3.forward, 
            targetLocalLookDir,
            Mathf.Deg2Rad * maxAngle,
            0);

        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        headBone.localRotation = Quaternion.Slerp(
            currentLocalRotation,
            targetLocalRotation,
            1 - Mathf.Exp(-trackingSpeed * Time.deltaTime));
    }

    #endregion

    #region Life-cycle Callbacks.

    private void LateUpdate()
    {
        if (type == 0)
        {
            if (aimConstraint != null)
            {
                GetComponent<Animator>().enabled = false;
            }
            CustomHeadTracking();
        }
        else
        {
            if (aimConstraint != null)
            {
                GetComponent<Animator>().enabled = true;
                aimConstraint.enabled = true;
            }
        }
    }

    #endregion
}

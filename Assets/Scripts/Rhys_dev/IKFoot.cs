using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKFoot : MonoBehaviour
{
    #region Variables.

    [SerializeField] private Transform transformFL;
    [SerializeField] private Transform transformFR;
    [SerializeField] private Transform transformBL;
    [SerializeField] private Transform transformBR;

    private Transform[] allTransforms;
    
    [SerializeField] private Transform targetTransformFL;
    [SerializeField] private Transform targetTransformFR;
    [SerializeField] private Transform targetTransformBL;
    [SerializeField] private Transform targetTransformBR;

    private Transform[] allTargetTransforms;

    [SerializeField] private GameObject rigFL;
    [SerializeField] private GameObject rigFR;
    [SerializeField] private GameObject rigBL;
    [SerializeField] private GameObject rigBR;

    private TwoBoneIKConstraint[] allIKConstraints;

    private bool[] allGroundHits;
    private float[] allCurves;

    private LayerMask walkableLayer;

    private float _maxHitDistance = 5f;
    private float _addedHeight = 3f;
    private LayerMask hitLayer;
    private Vector3[] allHitNormals;
    private float[] yOffset;
    
    #endregion

    #region Methods.

    private void CheckGround(out Vector3 hitPoint, out bool gotGroundSpherecastHit, out Vector3 hitNormal,
        out LayerMask hitLayer, out float currentHitDistance, Transform objectTransform, int checkForLayerMask,
        float maxHitDistance, float addedHeight)
    {
        Vector3 startPoint = objectTransform.position + new Vector3(0, addedHeight, 0);
        RaycastHit hit;

        if (checkForLayerMask == -1)
        {
            Debug.LogError("Layer doesn't exist.");
            gotGroundSpherecastHit = false;
            currentHitDistance = 0f;
            hitLayer = LayerMask.NameToLayer("Default");
            hitNormal = Vector3.up;
            hitPoint = objectTransform.position;
        }
        else
        {
            int layerMask = (1 << checkForLayerMask);

            if (Physics.SphereCast(startPoint, 0.2f, Vector3.down, out hit, maxHitDistance, layerMask,
                QueryTriggerInteraction.UseGlobal))
            {
                hitLayer = hit.transform.gameObject.layer;
                currentHitDistance = hit.distance - addedHeight;
                hitNormal = hit.normal;
                gotGroundSpherecastHit = true;
                hitPoint = hit.point;
            }
            else
            {
                gotGroundSpherecastHit = false;
                currentHitDistance = 0f;
                hitLayer = LayerMask.NameToLayer("Default");
                hitNormal = Vector3.up;
                hitPoint = objectTransform.position;
            }
        }
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector, Vector3 hitNormal)
    {
        return vector - hitNormal * Vector3.Dot(vector, hitNormal);
    }

    private void ProjectedAxisAngles(out float angleAboutX, out float angleAboutZ, Transform targetTransform, 
        Vector3 hitNormal)
    {
        Vector3 xAxisProjected = ProjectOnContactPlane(targetTransform.forward, hitNormal).normalized;
        Vector3 zAxisProjected = ProjectOnContactPlane(targetTransform.right, hitNormal).normalized;

        angleAboutX = Vector3.SignedAngle(targetTransform.forward, xAxisProjected, targetTransform.right);
        angleAboutZ = Vector3.SignedAngle(targetTransform.right, zAxisProjected, targetTransform.forward);
    }

    private void RotateFeet()
    {
        allCurves[0] = GetComponent<Animator>().GetFloat("FL");
        allCurves[1] = GetComponent<Animator>().GetFloat("FR");
        allCurves[2] = GetComponent<Animator>().GetFloat("BL");
        allCurves[3] = GetComponent<Animator>().GetFloat("BR");
        
        for (int i = 0; i < 4; ++i)
        {
            allIKConstraints[i].weight = allCurves[i];
            
            CheckGround(out Vector3 hitPoint, out allGroundHits[i], out Vector3 hitNormal, out hitLayer, out _,
                allTransforms[i], walkableLayer, _maxHitDistance, _addedHeight);
            allHitNormals[i] = hitNormal;

            if (allGroundHits[i] == true)
            {
                allTargetTransforms[i].position = new Vector3(allTransforms[i].position.x, hitPoint.y + yOffset[i],
                    allTransforms[i].position.z);
            }
            else
            {
                allTargetTransforms[i].position = allTransforms[i].position;
            }
        }
    }

    #endregion

    #region Life-cycle Callbacks.

    private void Start()
    {
        allTransforms = new[] {transformFL, transformFR, transformBL, transformBR};
        allTargetTransforms = new[] {targetTransformFL, targetTransformFR, targetTransformBL, targetTransformBR};
        allGroundHits = new bool[5];
        yOffset = new[] {0.18f, 0.18f, 0.06f, 0.06f};
        
        allIKConstraints = new TwoBoneIKConstraint[4];
        allIKConstraints[0] = rigFL.GetComponent<TwoBoneIKConstraint>();
        allIKConstraints[1] = rigFR.GetComponent<TwoBoneIKConstraint>();
        allIKConstraints[2] = rigBL.GetComponent<TwoBoneIKConstraint>();
        allIKConstraints[3] = rigBR.GetComponent<TwoBoneIKConstraint>();
        
        walkableLayer = LayerMask.NameToLayer("Walkable");

        allHitNormals = new Vector3[4];
        allCurves = new float[4];
    }

    private void FixedUpdate()
    {
        RotateFeet();
    }

    #endregion
}

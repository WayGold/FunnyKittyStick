using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    private LayerMask walkableLayer;

    private float _maxHitDistance = 5f;
    private float _addedHeight = 3f;

    private bool[] _allGroundSpherecastHits;
    private LayerMask _hitLayer;
    private Vector3[] _allHitNormals;
    private float _offset = 0.15f;

    private Animator _animator;
    private float[] _ikWeights;
    
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
        _ikWeights[0] = _animator.GetFloat("FL Weight");
        _ikWeights[1] = _animator.GetFloat("FR Weight");
        _ikWeights[2] = _animator.GetFloat("BL Weight");
        _ikWeights[3] = _animator.GetFloat("BR Weight");
            
        for (int i = 0; i < 4; ++i)
        {
            allIKConstraints[i].weight = /*_ikWeights[i]*/0f;
            
            CheckGround(out Vector3 hitPoint, out _allGroundSpherecastHits[i], out Vector3 hitNormal, 
                out _hitLayer, out _, allTransforms[i], walkableLayer, _maxHitDistance, _addedHeight);
            _allHitNormals[i] = hitNormal;

            if (_allGroundSpherecastHits[i])
            {
                if (i == 2 || i == 3)
                {
                    allTargetTransforms[i].position = new Vector3(allTransforms[i].position.x, hitPoint.y + _offset - 0.1f,
                        allTransforms[i].position.z);
                }
                else
                {
                    allTargetTransforms[i].position = new Vector3(allTransforms[i].position.x, hitPoint.y + _offset,
                        allTransforms[i].position.z);
                }

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
        _animator = GetComponent<Animator>();
        
        allTransforms = new[] {transformFL, transformFR, transformBL, transformBR};
        allTargetTransforms = new[] {targetTransformFL, targetTransformFR, targetTransformBL, targetTransformBR};

        allIKConstraints = new TwoBoneIKConstraint[4];
        allIKConstraints[0] = rigFL.GetComponent<TwoBoneIKConstraint>();
        allIKConstraints[1] = rigFR.GetComponent<TwoBoneIKConstraint>();
        allIKConstraints[2] = rigBL.GetComponent<TwoBoneIKConstraint>();
        allIKConstraints[3] = rigBR.GetComponent<TwoBoneIKConstraint>();
        
        walkableLayer = LayerMask.NameToLayer("Default");

        _allGroundSpherecastHits = new bool[5];

        _allHitNormals = new Vector3[4];

        _ikWeights = new float[4];
    }

    private void FixedUpdate()
    {
        RotateFeet();
    }

    #endregion
}

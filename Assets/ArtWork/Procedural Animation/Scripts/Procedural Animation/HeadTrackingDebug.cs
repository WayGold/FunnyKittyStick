using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HeadTrackingDebug : MonoBehaviour
{
    [SerializeField] private bool joyCon;

    [SerializeField] private MultiAimConstraint aim;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] public GameObject target;

    private void Awake()
    {
        TrackTarget();
    }

    public void TrackTarget()
    {
        GameObject gameObject = target;

        GetComponent<agent>().targetRB = gameObject.GetComponent<Rigidbody>();

        WeightedTransform weightedTransform = new WeightedTransform();
        weightedTransform.transform = gameObject.transform;
        weightedTransform.weight = 1;
        WeightedTransformArray weightedTransformArray = new WeightedTransformArray();
        weightedTransformArray.Add(weightedTransform);
        aim.data.sourceObjects = weightedTransformArray;

        rigBuilder.Build();
    }
}

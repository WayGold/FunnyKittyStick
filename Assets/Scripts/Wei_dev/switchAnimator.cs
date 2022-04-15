using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void _switchToMainAnimator()
    {
        Transform catTrans = GetComponent<Transform>();
        Vector3 currPos = catTrans.position;
        GetComponent<BoxCollider>().enabled = true;

        // Set Original Animation Controller Once Jumping is Finished
        Animator _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        _animator.runtimeAnimatorController = Resources.Load("Animator/Animator_Cat") as RuntimeAnimatorController;

        // Set Trans Back
        this.transform.position = currPos;

        // Start Agent
        GetComponent<agent>().enabled = true;
    }
}

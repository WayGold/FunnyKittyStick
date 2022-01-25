using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CatAnimationController : MonoBehaviour
{
    public OverrideTransform overrideTransform;
    
    private Animator _animator;

    private bool _isSitting;
    private bool _isTurningLeft;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _isSitting = false;
        _isTurningLeft = false;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            _animator.SetBool("IsWalking", true);
        }
        else
        {
            _animator.SetBool("IsWalking", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (_isSitting)
            {
                _animator.SetTrigger("StandUp");
                _isSitting = false;
            }
            else
            {
                _animator.SetTrigger("SitDown");
                _isSitting = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            _animator.SetTrigger("Attack");
        }

        /*if (Input.GetKey(KeyCode.A))
        {
            if (!_isTurningLeft)
            {
                overrideTransform.data.rotation -= new Vector3(0, 0, -60);
                _isTurningLeft = true;
            }
        }*/
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using FIMSpace.FSpine;
using UnityEngine;

public class CatAnimationController : MonoBehaviour
{
    private Animator _animator;
    private FSpineAnimator _spineAnimator;
    private Rigidbody _rigidbody;

    private bool _isSitting;
    private bool _isTurningLeft;

    public static CatAnimationController Instance;

    /// <summary>
    /// Sit down.
    /// </summary>
    public void SitDown()
    {
        if (!_isSitting)
        {
            _animator.SetTrigger("Sit");
            _isSitting = true;
        }
    }

    /// <summary>
    /// Stand up.
    /// </summary>
    public void StandUp()
    {
        if (!_isSitting)
        {
            _animator.SetTrigger("Stand");
            _isSitting = false;
        }
    }

    /// <summary>
    /// Move the cat.
    /// </summary>
    /// <param name="speed"> The moving speed. </param>
    public void Move(float speed)
    {
        _animator.SetFloat("Speed", speed);
    }

    /// <summary>
    /// Jump up.
    /// </summary>
    public void JumpUp()
    {
        _spineAnimator.SpineAnimatorAmount = 0;
        _animator.SetTrigger("JumpUp");
    }

    /// <summary>
    /// Jump forward.
    /// </summary>
    public void JumpForward()
    {
        SetSpineAnimationAmount(0);
        _animator.SetTrigger("JumpForward");
    }

    /// <summary>
    /// Attack.
    /// </summary>
    public void Attack()
    {
        _animator.SetTrigger("Attack");
    }

    public void SetSpineAnimationAmount(float amount)
    {
        _spineAnimator.SpineAnimatorAmount = amount;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
        _animator = GetComponent<Animator>();
        _spineAnimator = GetComponent<FSpineAnimator>();
        _rigidbody = GetComponent<Rigidbody>();

        _isSitting = false;
        _isTurningLeft = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (_isSitting)
            {
                _animator.SetTrigger("Stand");
                _isSitting = false;
            }
            else
            {
                _animator.SetTrigger("Sit");
                _isSitting = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //_animator.SetTrigger("JumpUp");
            //JumpForward();
            //_rigidbody.AddForce(transform.up * 6f + Vector3.Normalize(_rigidbody.velocity) * 4f, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            _animator.SetTrigger("Attack");
        }

        if (Input.GetKey(KeyCode.W))
        {
            _animator.SetFloat("Speed", 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            _animator.SetTrigger("JumpForward");
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

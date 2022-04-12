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
        // Set Original Animation Controller Once Jumping is Finished
        Animator _animator = GetComponent<Animator>();
        _animator.runtimeAnimatorController = Resources.Load("Animator/Animator_Cat") as RuntimeAnimatorController;

        // Set Trans Back
        if(this.name == "Cat.L.012")
            this.transform.position = new Vector3(-5.4f, 0f, -9.7f);

        // Start Agent
        GetComponent<agent>().enabled = true;
    }
}

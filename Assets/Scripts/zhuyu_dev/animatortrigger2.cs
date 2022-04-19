using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animatortrigger2 : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cat")
        {
            animator.SetTrigger("MoveTrain");
            var audioItem = GetComponent<AudioItem>();
            audioItem.Play();
        }
            

    }
}

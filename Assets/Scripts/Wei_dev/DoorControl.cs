using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    public GameObject door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //If collide with cat, open door
        if (other.gameObject.tag == "Cat")
        {
            door.GetComponent<Animator>().SetTrigger("Open");
        }
    }
}

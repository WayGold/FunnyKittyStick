using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destruction : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Cat")
        {
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
            foreach (var value in transforms)
            {
                value.gameObject.AddComponent<Rigidbody>();
            }
        }
    }
}

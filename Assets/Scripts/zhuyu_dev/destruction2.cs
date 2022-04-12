using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destruction2 : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "catstick")
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
            foreach (var value in transforms)
            {
                value.gameObject.AddComponent<Rigidbody>();
            }
        }
    }
}
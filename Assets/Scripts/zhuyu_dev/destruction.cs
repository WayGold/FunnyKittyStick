using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destruction : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "ground")
        {
            if(gameObject.GetComponent<Rigidbody>())
            {
                Destroy(gameObject.GetComponent<Rigidbody>());
                Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
                foreach (var value in transforms)
                {
                    value.gameObject.AddComponent<Rigidbody>();
                }

                if(gameObject.GetComponent<AudioItem>())
                {
                    var audioItem = GetComponent<AudioItem>();
                    audioItem.Play();
                }
            }
        }
    }
}
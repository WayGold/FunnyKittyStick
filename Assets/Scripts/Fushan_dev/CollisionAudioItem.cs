using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudioItem : MonoBehaviour
{


    [HideInInspector] public AudioItem audioitem;

    // Start is called before the first frame update
    void Start()
    {
        audioitem = GetComponent<AudioItem>();
    }

    // Update is called once per frame


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cat" || other.tag == "ground" || other.tag == "stickfish")
        {
            audioitem.Play();
        }


    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Cat" || 
            collision.gameObject.tag == "ground" || 
            collision.gameObject.tag == "stickfish")
        {
            audioitem.Play();
        }
    }
}

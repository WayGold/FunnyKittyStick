using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Cat")
        {
            Destroy(gameObject);
            GameObject.FindObjectOfType<HUDManager>().ColletItem();
        }
    }
}

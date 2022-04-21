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
            if(gameObject.name=="Cape")
            {
                other.GetComponentInChildren<Cloth>().gameObject.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
            GameObject.FindObjectOfType<HUDManager>().ColletItem();
        }
    }
}

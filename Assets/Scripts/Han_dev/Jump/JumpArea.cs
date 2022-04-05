using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpArea : MonoBehaviour
{
    public void DeActiveBoxCollider()
    {
        GetComponent<BoxCollider>().enabled = false;
        Invoke("ActiveBoxCollider", 10f);
    }
    void ActiveBoxCollider()
    {
        GetComponent<BoxCollider>().enabled = true;
    }
}

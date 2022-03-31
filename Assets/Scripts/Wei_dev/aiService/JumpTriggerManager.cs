using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTriggerManager : MonoBehaviour
{
    [SerializeField] public List<GameObject> allTriggers;

    // Update is called once per frame
    void Update()
    {
        foreach (var triggerBox in allTriggers)
        {
            print("debug");
            triggerBox.GetComponent<Collider>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        
    }
}

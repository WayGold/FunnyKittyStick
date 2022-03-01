using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTriggerManager : MonoBehaviour
{
    [SerializeField] public List<GameObject> allTriggers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var triggerBox in allTriggers)
        {
            triggerBox.GetComponent<Collider>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDebug : MonoBehaviour
{
    public Transform stick;
    public Transform mouseDetector;
    public bool allowMouseDebug=false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void FixedUpdate()
    {
        if(allowMouseDebug)
        {
            if (stick)
            {
                int layerMask = 1 << LayerMask.NameToLayer("MouseDetector"); //¼ì²âÃûÎª¡°MouseDetector¡±µÄ²ã
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                bool ishit = Physics.Raycast(ray, out hit, 1000, layerMask);
                if (ishit && hit.collider.tag == "MousePosDetector")
                {
                    stick.position = hit.point;
                }
            }

            Vector3 pos = mouseDetector.position;
            pos.y += Input.mouseScrollDelta.y * 0.5f;
            mouseDetector.position = pos;
        }

    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.M))
        {
            allowMouseDebug = !allowMouseDebug;
        }
    }
}

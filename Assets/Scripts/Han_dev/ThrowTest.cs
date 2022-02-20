using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowTest : MonoBehaviour
{
    public GameObject cat;
    public float G = 9.81f;

    //Used to compensate height
    public float addHeight = 1f;

    Vector3 targetPos;

    bool isTrowing = false;

    LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        targetPos = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
            ThrowCat();

        TryEndThrow();
    }
    void ThrowCat()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            print("RayHitObject:" + hit.collider.gameObject.name + " Position:" + hit.point);

            lineRenderer.SetPosition(0, cat.transform.position);
            lineRenderer.SetPosition(1, hit.point);

            Vector3 jumpVelpcity = CaculateTrowVelocity(hit.point);
            cat.GetComponent<Rigidbody>().velocity = jumpVelpcity;
            targetPos = hit.point;
            isTrowing = true;
        }
    }
    void TryEndThrow()
    {
        if (isTrowing == true && cat.transform.position.x >= targetPos.x - 0.5f && cat.transform.position.x <= targetPos.x + 0.5f
    && cat.transform.position.z >= targetPos.z - 0.5f && cat.transform.position.z <= targetPos.z + 0.5f)
        {
            cat.GetComponent<Rigidbody>().velocity = Vector3.zero;
            isTrowing = false;
        }
    }
    Vector3 CaculateTrowVelocity(Vector3 targetPos)
    {
        Vector3 catSize = cat.GetComponent<Collider>().bounds.size;
        float h = targetPos.y - (cat.transform.position.y-catSize.y/2)+addHeight;
        float s = Vector2.Distance(new Vector2(targetPos.x, targetPos.z),
            new Vector2(cat.transform.position.x, cat.transform.position.z));
        print("s="+ s);

        float v0 = Mathf.Sqrt(2 * G * h);
        float v1 = s / (Mathf.Sqrt((2 / G) * v0 - 2 * h / G));

        //horizontal velocity
        Vector3 vs = new Vector3(targetPos.x-cat.transform.position.x, 
                                    cat.transform.position.y, 
                                    targetPos.z-cat.transform.position.z).normalized * v1;
        //vertical velocity
        Vector3 vh = new Vector3(0, targetPos.y - cat.transform.position.y, 0).normalized*v0;

        return vh + vs;
    }
}

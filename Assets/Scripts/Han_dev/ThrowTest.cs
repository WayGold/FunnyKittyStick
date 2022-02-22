using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowTest : MonoBehaviour
{
    public GameObject cat;
    public LineRenderer lineRenderer;

    public float G = 9.81f;
    //Used to compensate height
    public float addHeight = 1f;

    Vector3 targetPos;
    bool isTrowing = false;

    void Start()
    {
        targetPos = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
            ThrowCat();

        TryEndThrow();
    }
    void TryEndThrow()
    {
        if (isTrowing == true && (cat.transform.position.x >= targetPos.x - 1f && cat.transform.position.x <= targetPos.x + 1f)
    && (cat.transform.position.z >= targetPos.z - 1f && cat.transform.position.z <= targetPos.z + 1f))
        {
            cat.GetComponent<Rigidbody>().velocity = Vector3.zero;
            isTrowing = false;
        }
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

            print("startPos:" + cat.transform.position);
            print("TargetPos: " + targetPos);

            targetPos = hit.point;
            isTrowing = true;

            Vector3 jumpVelpcity = CaculateTrowVelocity(hit.point);
            cat.GetComponent<Rigidbody>().velocity = jumpVelpcity;
        }
    }
    Vector3 CaculateTrowVelocity(Vector3 targetPos)
    {
        Vector3 catSize = cat.GetComponent<Collider>().bounds.size;

        float h = targetPos.y - (cat.transform.position.y-catSize.y/2)+addHeight;
        float s = Vector2.Distance(new Vector2(targetPos.x, targetPos.z),
            new Vector2(cat.transform.position.x, cat.transform.position.z));
        print("s="+ s+" h= "+h);

        return (h>=0)?UpThrow(h,s):DownThrow(h, s);
    }
    Vector3 UpThrow(float h,float s)
    {
        float v0 = Mathf.Sqrt(2 * G * h);
        float v1 = s / (Mathf.Sqrt((2 / G) * v0 - 2 * h / G));

        //horizontal velocity
        Vector3 vs = new Vector3(targetPos.x - cat.transform.position.x,
                                    0,
                                    targetPos.z - cat.transform.position.z).normalized * v1;
        //vertical velocity
        Vector3 vh = new Vector3(0, targetPos.y - cat.transform.position.y, 0).normalized * v0;

        print("UpThrow:"+"vh= " + vh + " vs=" + vs);
        return vh + vs;
    }
    Vector3 DownThrow(float h, float s)
    {
        float v0 = 0;
        float v1 = s / (Mathf.Sqrt((-2*h)/G));

        //horizontal velocity
        Vector3 vs = new Vector3(targetPos.x - cat.transform.position.x,
                                  0,
                                    targetPos.z - cat.transform.position.z).normalized * v1;
        //vertical velocity
        Vector3 vh = new Vector3(0, targetPos.y - cat.transform.position.y, 0).normalized * v0;

        print("DownThrow:"+"vh = " + vh + " vs=" + vs);
        return vs;
    }
}

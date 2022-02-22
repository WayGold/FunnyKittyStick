using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowTest : MonoBehaviour
{
    public GameObject cat;
    public LineRenderer lineRenderer;

    public float G = 9.81f;
    public float addHeight = 0.5f;
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

            Vector3 jumpVelpcity = CaculateTrowVelocity(cat,hit.point,5f);
            cat.GetComponent<Rigidbody>().velocity = jumpVelpcity;
        }
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
    Vector3 CaculateTrowVelocity(GameObject i_cat,Vector3 i_targetPos,float i_addHeight)
    {
        //if you need catSize offset, use this!
        Vector3 catSize = i_cat.GetComponent<Collider>().bounds.size;
        float h = i_targetPos.y - (i_cat.transform.position.y-catSize.y/2)+ i_addHeight;
        
        //if you don't need catSize offset, use this!
        //float h = i_targetPos.y - i_cat.transform.position.y + i_addHeight;
        
        float s = Vector2.Distance(new Vector2(i_targetPos.x, i_targetPos.z),
            new Vector2(i_cat.transform.position.x, i_cat.transform.position.z));

        return (h>=0)?UpThrow(i_cat, i_targetPos,h, s) :DownThrow(i_cat,i_targetPos, h, s);
    }
    Vector3 UpThrow( GameObject i_cat, Vector3 i_targetPos,float h, float s)
    {
        float v0 = Mathf.Sqrt(2 * G * h);
        float v1 = s / (Mathf.Sqrt((2 / G) * v0 - 2 * h / G));

        //horizontal velocity
        Vector3 vs = new Vector3(i_targetPos.x - i_cat.transform.position.x,
                                    0,
                                    i_targetPos.z - i_cat.transform.position.z).normalized * v1;
        //vertical velocity
        Vector3 vh = new Vector3(0, i_targetPos.y - i_cat.transform.position.y, 0).normalized * v0;

        print("UpThrow:"+"vh= " + vh + " vs=" + vs);
        return vh + vs;
    }
    Vector3 DownThrow(GameObject i_cat, Vector3 i_targetPos, float h, float s)
    {
        float v0 = 0;
        float v1 = s / (Mathf.Sqrt((-2*h)/G));

        //horizontal velocity
        Vector3 vs = new Vector3(i_targetPos.x - i_cat.transform.position.x,
                                  0,
                                    i_targetPos.z - i_cat.transform.position.z).normalized * v1;
        //vertical velocity
        Vector3 vh = new Vector3(0, i_targetPos.y - i_cat.transform.position.y, 0).normalized * v0;

        print("DownThrow:"+"vh = " + vh + " vs=" + vs);
        return vs;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    bool isStart = false;
    bool isDestory = false;
    bool shouldRotate = false;
    bool canGo = true;

    float timer = 0;
    float rotateDegree = 1;

    void Update()
    {
        if(isStart)
        {
            if (shouldRotate)
            {
                transform.Rotate(new Vector3(0, rotateDegree, 0));
            }
            else
            {
                if (canGo)
                    transform.Translate(Vector3.forward * Time.deltaTime * 3);
            }

            timer += Time.deltaTime;
            if (timer > 3)
            {
                rotateDegree = Random.Range(-0.5f, 0.5f);
                shouldRotate = !shouldRotate;
                if (shouldRotate) canGo = true;
                timer = 0;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Cat")
        {
            if (isDestory == false)
            {
                StartCoroutine(DestoryRobot(collision.gameObject));
                
                isDestory = true;
            }
        }
        else if(collision.gameObject.tag!="ground")
        {
            print("hit barrier");
            shouldRotate = true;
            canGo = false;
        }
    }
    
    IEnumerator DestoryRobot(GameObject cat)
    {
        yield return new WaitForSeconds(3);
        isStart = true; 

        yield return new WaitForSeconds(20);

        cat.transform.parent = null;
        cat.GetComponent<agent>().RobotBreak();
        //cat.GetComponent<Rigidbody>().AddForce(new Vector3(0, 200, 0), ForceMode.Acceleration);

        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (var value in transforms)
        {
            value.gameObject.AddComponent<Rigidbody>();
        }
        isStart = false;

        gameObject.GetComponent<SphereCollider>().enabled = false;
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }
}

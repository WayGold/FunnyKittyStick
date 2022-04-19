using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public bool isStart = false;
    public bool isDestory = false;
    public bool shouldRotate = false;

    public float timer = 0;
    public float rotateDegree = 1;

    public void Update()
    {
        if(isStart)
        {
            if (shouldRotate)
            {
                transform.Rotate(new Vector3(0, rotateDegree, 0));
            }
            else
            {
                transform.Translate(Vector3.forward * Time.deltaTime * 3);
            }

            timer += Time.deltaTime;
            if (timer > 3)
            {
                rotateDegree = Random.Range(-0.5f, 0.5f);
                shouldRotate = !shouldRotate;
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
                //StartCoroutine(StartRobot(collision.gameObject));
                isDestory = true;
            }
        }
        else if(collision.gameObject.tag!="ground")
        {
            print("hit barrier");
            shouldRotate = true;
        }
    }

    public void StartRobotPower(GameObject cat)
    {
        StartCoroutine(DestoryRobot(cat));
        var audioItem = GetComponent<AudioItem>();
        audioItem.Play();
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

        var audioItem = GetComponent<AudioItem>();
        AudioItem.FadeOutAudioSource(audioItem.GetCurrentAudioSource(), 0.3f);


        gameObject.GetComponent<SphereCollider>().enabled = false;
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }
}

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

    bool setCatPos = false;
    Vector3 catOffset;
    GameObject Cat;
    private void Start()
    {
        catOffset = new Vector3(0, 0.2f, 0);
    }
    public void Update()
    {
        if(isStart)
        {
            if (shouldRotate)
            {
                transform.Rotate(new Vector3(0, rotateDegree, 0) * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector3.forward * Time.deltaTime * 5);
            }

            timer += Time.deltaTime;
            if (timer > 3)
            {
                rotateDegree =-20;
                shouldRotate = !shouldRotate;
                timer = 0;
            }
        }

        if(setCatPos)
        {
            Cat.transform.position = gameObject.transform.position + catOffset;
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
        //else if(collision.gameObject.tag!="ground")
        //{
        //    print("hit barrier");
        //    shouldRotate = true;
        //}
    }

    public void StartRobotPower(GameObject cat)
    {
        StartCoroutine(DestoryRobot(cat));
        var audioItem = GetComponent<AudioItem>();
        audioItem.Play();

        Cat = cat;
        //catOffset = cat.transform.position- gameObject.transform.position;
    }
    
    IEnumerator DestoryRobot(GameObject cat)
    {
        setCatPos = true;
        yield return new WaitForSeconds(3);
        isStart = true;

        yield return new WaitForSeconds(7);
        setCatPos = false;

        yield return new WaitForSeconds(2);

        isStart = false;

        cat.transform.parent = null;
        cat.GetComponent<agent>().RobotBreak();

        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (var value in transforms)
        {
            value.gameObject.AddComponent<Rigidbody>();
        }


        var audioItem = GetComponent<AudioItem>();
        AudioItem.FadeOutAudioSource(audioItem.GetCurrentAudioSource(), 0.3f);


        gameObject.GetComponent<SphereCollider>().enabled = false;
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }
}

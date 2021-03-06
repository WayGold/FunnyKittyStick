using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    // 需要跟随的目标对象
    public Transform target;

    //private Vector3 target;

    //public Transform cat;
    //public Transform stick;

    //public float minSize=6;
    //public float coefficient;

    public Collider targetEffectiveCollider;
    

    // 需要锁定的坐标（可以实时生效）
    public bool freazeX, freazeY, freazeZ;

    // 跟随的平滑时间（类似于滞后时间）
    public float smoothTime = 0.3F;
    private float xVelocity, yVelocity, zVelocity = 0.0F;

    // 跟随的偏移量
    private Vector3 offset;

    // 全局缓存的位置变量
    private Vector3 oldPosition;

    // 记录初始位置
    private Vector3 startPosition;

    void Start()
    {
        //cat = GameObject.FindGameObjectWithTag("Cat").GetComponent<Transform>();
        //stick = GameObject.FindGameObjectWithTag("Stick").GetComponent<Transform>();
        //target= (cat.position + stick.position) / 2;
        target = GameObject.FindGameObjectWithTag("Cat").GetComponent<Transform>();
        startPosition = transform.position;
        offset = transform.position - target.transform.position;
    }

    void LateUpdate()
    {
        //target = GameObject.FindGameObjectWithTag("Cat").GetComponent<Transform>();
        //offset = transform.position - target.transform.position;

        //// If outside the effective collider, don't follow the target
        //if (!targetEffectiveCollider.bounds.Contains(target.position))
        //{
        //    return;
        //}

        //target = (cat.position + stick.position) / 2;

        oldPosition = transform.position;
        if (!freazeX)
        {
            oldPosition.x = Mathf.SmoothDamp(transform.position.x, target.transform.position.x + offset.x, ref xVelocity, smoothTime);
        }

        if (!freazeY)
        {
            oldPosition.y = Mathf.SmoothDamp(transform.position.y, target.transform.position.y + offset.y, ref yVelocity, smoothTime);
        }

        if (!freazeZ)
        {
            oldPosition.z = Mathf.SmoothDamp(transform.position.z, target.transform.position.z + offset.z, ref zVelocity, smoothTime);
        }
        //Camera.main.orthographicSize = (Vector3.Distance(cat.position , stick.position) / 2 * coefficient > minSize) ? Vector3.Distance(cat.position , stick.position) / 2 * coefficient : minSize;
        transform.position = oldPosition;
    }

    /// <summary>
    /// 用于重新开始游戏时直接重置相机位置
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}

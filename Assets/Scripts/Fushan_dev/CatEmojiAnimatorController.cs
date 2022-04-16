using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatEmojiAnimatorController : MonoBehaviour
{


    public Animator catEmojiAnimator;
    public float catEmojiDuration;
    // Start is called before the first frame update
    void Start()
    {
        /*
        var childNum = transform.childCount;
        for(int i = 0; i < childNum; ++i)
        {
            var child = transform.GetChild(i);
            var animator = child.GetComponentInChildren<Animator>();
            if (child != null)
            {
                catEmojiAnimator = animator;    
                Debug.Log("Get Animator: " + catEmojiAnimator.name);
                return;
            }
            
        }
        Debug.Log("Fail to get the cat emoji animator");
        */

    }

    // Update is called once per frame

    public void PlayCatEmoji(string name)
    {
        catEmojiAnimator.SetBool(name, true);
        StartCoroutine(HoldUIAnimation(catEmojiAnimator, name, catEmojiDuration));
    }

    IEnumerator HoldUIAnimation(Animator _animator, string name, float duration)
    {
        yield return new WaitForSeconds(duration);
        _animator.SetBool(name, false);
    }

}

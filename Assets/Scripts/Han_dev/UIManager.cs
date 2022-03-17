using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Cat Emoji UI")]
    public Animator catUIAnimator;
    public Animator fishUIAnimator;
    public RectTransform catUITransform;

    private void Start()
    {
        //InitializeAnimator();
    }
    void InitializeAnimator()
    {
        if (!catUIAnimator)
            catUIAnimator=GameObject.FindGameObjectWithTag("CatUI").GetComponent<Animator>();
        if (!fishUIAnimator)
            fishUIAnimator=GameObject.FindGameObjectWithTag("FishUI").GetComponent<Animator>();
        if(!catUITransform)
            catUITransform=GameObject.FindGameObjectWithTag("CatUI").GetComponent<RectTransform>();
    }
    void PlayUIAnimation(Animator _animator, string boolName, float duration)
    {
        _animator.SetBool(boolName, true);
        StartCoroutine(HoldUIAnimation(_animator, boolName, duration));
    }

    IEnumerator HoldUIAnimation(Animator _animator, string boolName, float duration)
    {
        yield return new WaitForSeconds(duration);
        _animator.SetBool(boolName, false);
    }

    // --- Public Event

    public void OnCatAttack()
    {
        // PlayCatAudio(2);
        // PlaySFXAttack();
        PlayUIAnimation(catUIAnimator, "Trying", 2);
    }

    public void OnCatJumpForward()
    {
        //PlayCatAudio(0);
        // PlaySFXJump();
        PlayUIAnimation(fishUIAnimator, "Highlight", 2f);
    }

    public void OnCatSit()
    {
        //PlayCatAudio(2);
        PlayUIAnimation(catUIAnimator, "Event", 1.2f);
    }

    public void OnCatJumpUp()
    {
        // PlayCatAudio(3);
        // PlaySFXJump();
        PlayUIAnimation(catUIAnimator, "Event", 1.5f);
    }

    public void OnCatStand()
    {
        // PlayCatAudio(1);
        PlayUIAnimation(catUIAnimator, "Trying", 1.5f);
    }
}

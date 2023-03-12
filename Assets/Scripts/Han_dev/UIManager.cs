using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Cat Emoji UI")]
    public Animator catUIAnimator;
    public Animator fishUIAnimator;
    public RectTransform catUITransform;

    public Animator cat_gray;
    public Animator cat_white;
    public Animator cat_short;
    private void Start()
    {
        InitializeAnimator();
    }
    void InitializeAnimator()
    {
        if (!catUIAnimator)
        {
            switch (CatSelection.CAT_SELECTED)
            {
                case 0:
                    catUIAnimator = cat_gray;
                    break;
                case 1:
                    catUIAnimator = cat_short;
                    break;
                case 2:
                    catUIAnimator = cat_white;
                    break;
                default:
                    break;
            }
        }
        if (!fishUIAnimator)
        {
            if (GameObject.FindGameObjectWithTag("FishUI"))
            {
                fishUIAnimator = GameObject.FindGameObjectWithTag("FishUI").GetComponent<Animator>();
            }
        }
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
        PlayUIAnimation(fishUIAnimator, "Highlight", 0.5f);
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

    public void CatLike()
    {
        PlayUIAnimation(catUIAnimator, "Like", 2f);
    }
}

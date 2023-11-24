using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectController : MonoBehaviour
{
    public bool isFlipEffect = false;

    private Animator anim;
    private SpriteRenderer parrentRen, mRen;

    void Start()
    {
        anim = GetComponent<Animator>();
        parrentRen = transform.parent.GetComponent<SpriteRenderer>();
        mRen = GetComponent<SpriteRenderer>();
    }

    public void ChangeAnimationState(int state)
    {
        anim.SetInteger("State", state);
        anim.SetTrigger("Jump");
    }

    private void FixedUpdate()
    {
        if (isFlipEffect)
        {
            if (parrentRen.flipX == mRen.flipX)
            {
                mRen.flipX = !parrentRen.flipX;
            }
        }
        else
        {
            if (parrentRen.flipX != mRen.flipX)
            {
                mRen.flipX = parrentRen.flipX;
            }
        }
    }
}

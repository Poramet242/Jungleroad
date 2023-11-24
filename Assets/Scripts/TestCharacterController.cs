using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCharacterController : MonoBehaviour
{
    public Sprite upSprite, rightSprite, jumpUpSprite, jumpRightSprite;
    private SpriteRenderer ren;
    private Coroutine changeSpriteCo;

    public CharacterEffectController CEC;

    private void Start()
    {
        ren = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //left
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (changeSpriteCo != null)
                StopCoroutine(changeSpriteCo);
            ren.flipX = true;
            changeSpriteCo = StartCoroutine(changeSpriteDelay(jumpUpSprite, upSprite));
            CEC?.ChangeAnimationState(3);
        }
        //right
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (changeSpriteCo != null)
                StopCoroutine(changeSpriteCo);
            ren.flipX = false;
            changeSpriteCo = StartCoroutine(changeSpriteDelay(jumpRightSprite, rightSprite));
            CEC?.ChangeAnimationState(2);
        }
        //up
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (changeSpriteCo != null)
                StopCoroutine(changeSpriteCo);
            ren.flipX = false;
            changeSpriteCo = StartCoroutine(changeSpriteDelay(jumpUpSprite, upSprite));
            CEC?.ChangeAnimationState(3);
        }
        //down
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (changeSpriteCo != null)
                StopCoroutine(changeSpriteCo);
            ren.flipX = true;
            changeSpriteCo = StartCoroutine(changeSpriteDelay(jumpRightSprite, rightSprite));
            CEC?.ChangeAnimationState(2);
        }
    }

    IEnumerator changeSpriteDelay(Sprite jump, Sprite normal)
    {
        ren.sprite = jump;
        yield return new WaitForSeconds(0.5f);
        ren.sprite = normal;
    }
}

using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class Log : Block
{
    private bool active = false;

    int movingDirection = 1;
    float movingSpeed = 0;
    int size = 1;
    Vector3 moveVector = Vector3.zero;

    public override void Setup(JSONNode data)
    {
        BlockCollider.isTrigger = false;

        movingDirection = data["direction"].AsInt;

        movingSpeed = data["speed"].AsFloat;

        moveVector = new Vector3(movingDirection * -1,0,0);
    }

    public override void StartActive()
    {
        active = true;
    }

    public override void StopActive()
    {
        active = false;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if(active){
            if(movingSpeed > 0){
                transform.Translate(moveVector * movingSpeed * Time.deltaTime);
                if(movingDirection < 0){
                    if (transform.localPosition.x > (4 + size)) {
                        transform.localPosition = new Vector3(-4, 0, 0);
                    }
                }
                if(movingDirection > 0){
                    if (transform.localPosition.x < (-4 - size)) {
                        transform.localPosition = new Vector3(4, 0, 0);
                    }
                }
            }
        }
    }
}

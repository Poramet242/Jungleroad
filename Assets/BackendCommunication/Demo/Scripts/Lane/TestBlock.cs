using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System;

public class TestBlock : Block
{
    [SerializeField]
    int movingDirection = 1;
    [SerializeField]
    float movingSpeed = 0;
    [SerializeField]
    int size = 1;
    [SerializeField]
    Vector3 moveVector = Vector3.zero;
    [SerializeField]
    bool active = false;

    bool isStart = false;

    DateTime startTime;

    public override void Setup(JSONNode data)
    {
        BlockCollider.isTrigger = true;
        moveVector = new Vector3(movingDirection * -1,0,0);
    }

    public override void StartActive()
    {
        throw new System.NotImplementedException();
    }

    public override void StopActive()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup(null);
    }

    double diffInSeconds;
    void FixedUpdate()
    {
        if(active){
            if(!isStart){
                isStart = true;
                startTime = DateTime.Now;
            }
            if(movingSpeed > 0){
                diffInSeconds = (DateTime.Now - startTime).TotalSeconds;
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
                Debug.Log($"ActiveTime: {diffInSeconds} x: {transform.localPosition.x}");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class Gator : Block
{
    public Transform bodyPrefab;
    public Transform bodyParent;

    private bool active = false;

    int movingDirection = 1;
    float movingSpeed = 0;
    int size = 1;
    Vector3 moveVector = Vector3.zero;

    public override void Setup(JSONNode data)
    {
        BlockCollider.isTrigger = true;

        movingDirection = data["direction"].AsInt;

        movingSpeed = data["speed"].AsFloat;

        moveVector = new Vector3(movingDirection * -1,0,0);

        size = data["size"].AsInt;
        /*for(int i = 0; i < size; i++){
            Transform bodyBlock = Instantiate<Transform>(bodyPrefab, Vector3.zero, Quaternion.identity);
            bodyBlock.parent = bodyParent;
            bodyBlock.localPosition = new Vector3(((i+1)*movingDirection), 0, 0);
            bodyBlock.GetComponent<Collider>().isTrigger = true;
        }*/
    }

    public override void StartActive()
    {
        active = true;
    }

    public override void StopActive()
    {
        active = false;
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

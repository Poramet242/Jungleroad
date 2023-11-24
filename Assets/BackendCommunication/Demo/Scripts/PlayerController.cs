using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed;
    bool isMoving;
    Vector3 targetPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void MoveForward(){
        if(isMoving) return;
        targetPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 1);
        isMoving = true;
    }

    public void MoveBack(){
        if(isMoving) return;
        targetPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 1);
        isMoving = true;
    }

    public void MoveLeft(){
        if(isMoving) return;
        targetPos = new Vector3(transform.localPosition.x - 1, transform.localPosition.y, transform.localPosition.z);
        isMoving = true;
    }

    public void MoveRight(){
        if(isMoving) return;
        targetPos = new Vector3(transform.localPosition.x + 1, transform.localPosition.y, transform.localPosition.z);
        isMoving = true;
    }

    void Update()
    {
        if(isMoving){
            float step =  MoveSpeed * Time.deltaTime;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, step);

            if (Vector3.Distance(transform.localPosition, targetPos) < 0.001f)
            {
                transform.localPosition = targetPos;
                isMoving = false;
            }
        }
    }
}

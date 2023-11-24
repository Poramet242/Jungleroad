using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform Target;

    public float FollowSpeed;

    public float FollowThreshold = 0.001f;

    private Vector3 camPos;
    private float step;

    // Update is called once per frame
    void Update()
    {
        step = FollowSpeed * Time.deltaTime;
        camPos = new Vector3(transform.localPosition.x, transform.localPosition.y, Target.localPosition.z);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, camPos, step);

        if (Vector3.Distance(transform.localPosition, camPos) < FollowThreshold)
        {
            transform.localPosition = camPos;
        }
    }
}

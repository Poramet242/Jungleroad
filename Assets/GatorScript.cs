using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatorScript : MonoBehaviour,IGameWorldUpdatable
{
    public Transform startPoint, endPoint, resetPoint;
    public Transform playerPoint;
    public float speed;
    public int dis;
    public int gatorPos;
    public float gatorDelay, useTime;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 pos0;
    private Vector3 dir;
    [SerializeField]
    private bool enabledDebug;

#if UNITY_EDITOR
    [SerializeField] float totalTravelingDistance;
#endif

    private void Start()
    {
        int numGs = -9;
        int numGe = 18;
        var gs = GridManager.Instance.GridToVector(numGs - 4, 0);
        var ge = GridManager.Instance.GridToVector(numGe - 4, 0);
        startPos = transform.parent.TransformPoint(gs);
        endPos = transform.parent.TransformPoint(ge);

        var p = Vector3.Lerp(startPos, endPos, (float)(gatorPos + 9) / 27);
        transform.position = p;
        pos0 = transform.position;

        switch (dis)
        {
            case 1:
                resetPoint = startPoint;
                break;
            case -1:
                resetPoint = endPoint;
                break;
            case 0:
                resetPoint = startPoint;
                break;
            default:
                break;
        }
        dir = dis * (endPoint.position - startPoint.position).normalized;
        GameWorld.addMovebableObject(this);
    }

    void OnDestroy() {
        GameWorld.removeMovableObject(this);
    }

    public void GameUpdate(float gameTime)
    {
        const string gatorTag = "[GatorScript::GameUpdate]";
        float D = Vector3.Distance(startPos, endPos);
        float distance = speed * gameTime;

#if UNITY_EDITOR
        totalTravelingDistance = distance;
#endif

        Vector3 pos = pos0;
        if (gameTime == 0)
        {
            gameObject.transform.position = pos;
            return;
        }

        float d0 = 0;
        float d = 0;
        float r = 0;
        float remain = 0;

        if (dis > 0)
        {
            d0 = (pos0 - startPos).magnitude;
            d = d0 + distance;
            r = Mathf.Floor(d / D);
            remain = d - (r * D);
            pos = startPos + (remain * dir);
        }
        else if (dis < 0)
        {
            d0 = (pos0 - endPos).magnitude;
            d = d0 + distance;
            r = Mathf.Floor(d / D);
            remain = d - (r * D);
            pos = endPos + (remain * dir);
        }

#if UNITY_EDITOR
        if (enabledDebug)
        {
            Debug.Log(string.Join("\n",
            $"{gatorTag} pos0: {pos0}",
            $"gameTime: {gameTime}",
            $"startPoint: {startPos}",
            $"endPoint: {endPos}",
            $"dir: {dir}",
            $"d0: {d0} d: {d} r: {r} remain: {remain}"));
        }
#endif

        gameObject.transform.position = pos;

    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0), transform.position + new Vector3(0, 0.5f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0), transform.position);

        Gizmos.color = new Color(255, 128, 0);
        if (dis > 0)
        {
            Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0), transform.position + new Vector3(0, 0.5f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.75f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.75f, 0), transform.position + new Vector3(-1f, 0.5f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-1f, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.25f, 0));
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0.25f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0), transform.position + new Vector3(1f, 0, 0));
            Gizmos.DrawLine(transform.position + new Vector3(1f, 0, 0), transform.position + new Vector3(0.5f, -0.25f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.25f, 0), transform.position);
        }
    }
#endif
}

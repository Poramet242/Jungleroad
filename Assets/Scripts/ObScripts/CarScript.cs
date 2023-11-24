using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour, IGameWorldUpdatable
{
    public Transform startPoint, endPoint, resetPoint;

    public float speed;
    public int dir;
    public int carPos;
    public float carDelay, useTime;
    [SerializeField] Animator animator, animator2;

    private Vector3 startPos;
    private Vector3 endPos;
    
    private Vector3 pos0;
    private Vector3 dis;

    [SerializeField] bool enabledDebug;

#if UNITY_EDITOR
    [SerializeField] float totalTravelingDistance;
#endif

    public int obInLane;
    private void Start()
    {
        int numGs = -9;
        int numGe = 18;
        var gs = GridManager.Instance.GridToVector(numGs - 4, 0);
        var ge = GridManager.Instance.GridToVector(numGe - 4, 0);
        startPos = transform.parent.TransformPoint(gs);
        endPos = transform.parent.TransformPoint(ge);

        var p = Vector3.Lerp(
            a: startPos, 
            b: endPos, 
            t: (float)(carPos + 9) / 27); // GridManager.Instance.GridToVector(carPos - 4, 0);
        transform.position = p;
        pos0 = transform.position;

        switch (dir)
        {
            case 1:
                resetPoint = startPoint;
                break;
            case -1:
                resetPoint = endPoint;
                break;
            default:
                break;
        }
        dis = dir * (endPoint.position - startPoint.position).normalized;
        GameWorld.addMovebableObject(this);

        // CarActive();
    }

    public void GameUpdate(float gameTime) {
        const string logTag = "[CarScript::GameUpdate]";
        float D = Vector3.Distance(startPos, endPos);
        float distance = speed * gameTime;

#if UNITY_EDITOR
        // for debug propose...
        totalTravelingDistance = distance;
#endif
        
        Vector3 pos = pos0; // just initial
        if (gameTime == 0) {
            gameObject.transform.position = pos;
            return;
        }

        float d0 = 0;
        float d = 0;
        float r = 0;
        float remain = 0;

        if (dir > 0) {
            d0 = (pos0 - startPos).magnitude;
            d = d0 + distance;
            r = Mathf.Floor(d/D);
            remain = d - (r * D);
            pos = startPos + (remain * dis);
        } else if (dir < 0) {
            d0 = (pos0 - endPos).magnitude;
            d = d0 + distance;
            r = Mathf.Floor(d/D);
            remain = d - (r * D);
            pos = endPos + (remain * dis);
        }

#if UNITY_EDITOR
        if (enabledDebug) {
            Debug.Log(string.Join("\n", 
            $"{logTag} pos0: {pos0}", 
            $"gameTime: {gameTime}",
            $"startPoint: {startPos}",
            $"endPoint: {endPos}",
            $"dir: {dir}",
            $"d0: {d0} d: {d} r: {r} remain: {remain}"));   
        }
#endif
      
        gameObject.transform.position = pos;
    }
    
    // void Update()
    // {
    //     Vector2 direction = (endPoint.position - startPoint.position).normalized;
    //     gameObject.transform.Translate(Dis * direction * speed * Time.deltaTime);
    // }

    void OnDestroy() {
        GameWorld.removeMovableObject(this);
    }
    
    [Obsolete("...")]
    public void CarActive()
    {
        StartCoroutine("CarReset");
        gameObject.SetActive(true);
    }

    [Obsolete("...")]
    IEnumerator CarReset()
    {
        yield return new WaitForSeconds(carDelay);

        transform.localPosition = resetPoint.localPosition;

        carDelay = useTime;
        StartCoroutine("CarReset");
    }
#if UNITY_EDITOR
    void OnDrawGizmos() // Car
    {
        Gizmos.color = new Color(0,0,255f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0), transform.position + new Vector3(0, 0.5f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0), transform.position);
    }
#endif
}

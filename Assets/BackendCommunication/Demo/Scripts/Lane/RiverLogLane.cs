using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using TMPro;
using System.Linq;

public class RiverLogLane : Lane
{
    public Log LogPrefab;
    public Transform MovingBlockParent;
    public TMP_Text text;

    private int moveDirection;
    private double moveSpeed;
    private JSONArray logPosition;

    private int startIndexPos = -4;
    private List<Log> logs;
    [SerializeField] private List<LogScript> logSorted;
    [SerializeField] List<float> LogDelayTime = new List<float>();
    //---------------------------------------------------------------
    [Header("Edit Attribuild to Dirt Lane")]
    [SerializeField] Transform startPoint, endPoint;
    [SerializeField] float useTime;
    private float rndSpeed;
    private int moveDis;
    LogScript LogScript;
    private int mirrorNum = 0;

    [Header("Debug GridPivot")]
    [SerializeField]
    private GameObject gridPivot;
    public override void DeActive()
    {
        for(int i = 0; i < logs.Count; i++){
            DestroyImmediate(logs[i].gameObject);
        }

        mirrorNum = 0;

        logs.Clear();

        base.DeActive();
    }

    public override void Setup(JSONNode jsonData, int lIndex)
    {
        base.Setup(jsonData, lIndex);

        logPosition = jsonData["LogPosition"].AsArray; // ตำแหน่งขอนไม้แต่ละจุดขนาด 1 block
        moveDirection = jsonData["MoveDirection"].AsInt; // ทิศทางเคลื่อนที่
        moveSpeed = jsonData["MoveSpeed"].AsDouble; // ความเร็วเคลื่อนที่
        text.text = jsonData["PatternID"].ToString();
        //TODO จัดการ spawn ขอนไม้
        // ลบ Demo ด้านล่างทิ้งแล้วใช้วิธีของ client จริงๆได้เลย
#if UNITY_EDITOR
        for (int i = 0; i < Floors.Length; i++)
        {
            Floors[i].gameObject.SetActive(true);
            //Add
            var gridPivotSpawn = Instantiate(gridPivot, Floors[i].transform);
            GridDisplay gd = gridPivotSpawn.GetComponent<GridDisplay>();
            gd.myGridPoint = GridManager.Instance.VectorToGrid(Floors[i].transform.position.x,
            Floors[i].transform.position.y) + new Vector2Int(4, 4);
            gridPivotSpawn.transform.parent = null;
        }
#endif
        logs = new List<Log>();
        logSorted = new List<LogScript>();

        JSONObject logSetting = new JSONObject();
        logSetting["direction"] = moveDirection;
        logSetting["speed"] = moveSpeed;
        rndSpeed = logSetting["speed"];
        moveDis = logSetting["direction"];

        logSetting["size"] = 0;

        for (int i = 0; i < logPosition.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 mirrorPos = new Vector3(
                    GridManager.Instance.GridToVector(((j - 1) * 9), 0).x,
                    GridManager.Instance.GridToVector(((j - 1) * 9), 0).y);

                Log log = Instantiate<Log>(LogPrefab, Vector3.zero, Quaternion.identity);
                log.transform.parent = MovingBlockParent;
                log.transform.localPosition = new Vector3(
                    GridManager.Instance.GridToVector(logPosition[i] - 4, 0).x,
                    GridManager.Instance.GridToVector(logPosition[i] - 4, 0).y, 
                    0) + mirrorPos;

                log.Setup(logSetting);
                logs.Add(log);
                TerrainLogLane(log);

                LogScript.logPos = (logPosition[i] + (j - 1) * 9);
                LogScript.logDelay = LogDelayTime[mirrorNum];
                LogScript.useTime = useTime;
                LogScript.gameObject.name = $"log[{LogScript.logPos}]";
                mirrorNum++;
            }
        }

        foreach (var item in logs)
        {
            logSorted.Add(item.GetComponent<LogScript>());
            if (moveDirection == 1)
                logSorted = logSorted.OrderBy(w => -w.logPos).ToList();
            else
                logSorted = logSorted.OrderBy(w => w.logPos).ToList();
            item.GetComponent<LogScript>().obInLane = logs.Count;
        }
        GridDatabase.Instance.logDatas.AddRange(logSorted);

        gameObject.SetActive(true);
    }

    public override void StartActive()
    {
        base.StartActive();

        for(int i = 0; i < logs.Count; i++){
            logs[i].StartActive();
        }
    }

    public override void StopActive()
    {
        base.StopActive();

        for(int i = 0; i < logs.Count; i++){
            logs[i].StopActive();
        }
    }
    public void TerrainLogLane(Log logs)
    {
        switch (moveDis)
        {
            case -1:
                CalculateUseTime(logs, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                LogDelayTime.Add(Vector2.Distance(logs.transform.localPosition, startPoint.localPosition) / rndSpeed);
                //=======================================
                break;
            case 0:
                CalculateUseTime(logs, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                LogDelayTime.Add(Vector2.Distance(logs.transform.localPosition, startPoint.localPosition) / rndSpeed);
                break;
            case 1:
                CalculateUseTime(logs, Vector2.Distance(startPoint.localPosition, endPoint.localPosition)); // Change position => local
                //================= new ================= R
                LogDelayTime.Add(Vector2.Distance(endPoint.localPosition, logs.transform.localPosition) / rndSpeed);
                //=======================================
                break;
        }
    }
    public void CalculateUseTime(Log logs, float dist)
    {
        useTime = dist / rndSpeed;
        LogScript = logs.GetComponent<LogScript>();
        LogScript.startPoint = startPoint;
        LogScript.endPoint = endPoint;
        LogScript.speed = rndSpeed;
        LogScript.dis = moveDis;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var c = Gizmos.color;
        Gizmos.color = Color.green;
        for (int i = -9; i <= 17; i++)
        {
            var p = GridManager.Instance.GridToVector(i - 4, 0);
            Gizmos.DrawWireSphere(transform.TransformPoint(p), 0.2f);
        }
        Gizmos.color = c;
    }
#endif
}

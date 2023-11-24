using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class RiverLane : Lane
{
    public Gator GatorPrefab;
    public Log LogPrefab;
    public Transform MovingBlockParent;
    private int startIndexPos = -4;
    private List<Log> logs;
    private List<Gator> gators;

    private float rndSpeedLog;
    private float rndSpeedGator;
    private float moveDisLog;
    private float moveDisGator;
    [Header("Star Position and End Position")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    //--------------------------------------------------------
    [Header("Edit Attribuild to River Log Lane")]
    [SerializeField] List<Log> logList = new List<Log>();
    Queue<Log> logQueue = new Queue<Log>();
    [SerializeField] float useTimeLog, delayTimeLog;
    [SerializeField] int queueLog;
    //--------------------------------------------------------
    [Header("Edit Attribuild to River Gator Lane ")]
    [SerializeField] List<Gator> gatorsList = new List<Gator>();
    Queue<Gator> gatorQueue = new Queue<Gator>();
    [SerializeField] float useTimeGator, dalayTimeGator;
    [SerializeField] int queueGator;
    //--------------------------------------------------------
    LogScript logScript;
   public override void Setup(JSONNode jsonData, int lIndex)
    {
        base.Setup(jsonData, lIndex);
        
        JSONArray logPosition = jsonData["LogPosition"].AsArray;
        JSONArray gatorPosition = jsonData["CrocPosition"].AsArray;

        int moveDirection = jsonData["MoveDirection"].AsInt;
        double moveSpeed = jsonData["MoveSpeed"].AsDouble;

        logs = new List<Log>();
        gators = new List<Gator>();

        JSONObject logSetting = new JSONObject();
        logSetting["direction"] = moveDirection;
        logSetting["speed"] = moveSpeed;
        rndSpeedLog = logSetting["speed"];
        moveDisLog = logSetting["direction"];

        logSetting["size"] = 0;
        for (int i = 0; i < logPosition.Count; i++)
        {
            Log log = Instantiate<Log>(LogPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log(log);
            log.transform.parent = MovingBlockParent;
            log.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(logPosition[i], 0).x, 0, 0);
            log.Setup(logSetting);
            logs.Add(log);

            TerrainRiverLane(moveDisLog);
            CalculateUseTimeLog(log, moveDisLog);
        }

        JSONObject gatorSetting = new JSONObject();
        gatorSetting["direction"] = moveDirection;
        gatorSetting["speed"] = moveSpeed;
        rndSpeedGator = gatorSetting["speed"];
        moveDisGator = gatorSetting["direction"];

        gatorSetting["size"] = 1;
        for (int i = 0; i < gatorPosition.Count; i++)
        {
            Gator gator = Instantiate<Gator>(GatorPrefab, Vector3.zero, Quaternion.identity);
            gator.transform.parent = MovingBlockParent;
            gator.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(gatorPosition[i], 0).x, 0, 0);
            gator.Setup(gatorSetting);
            gators.Add(gator);

            TerrainRiverLane(moveDisGator);
            CalculateUseTimeGator(gator, moveDisGator);

        }
    }

    public override void StartActive()
    {
        base.StartActive();

        for (int i = 0; i < logs.Count; i++)
        {
            logs[i].StartActive();
        }

        for (int i = 0; i < gators.Count; i++)
        {
            gators[i].StartActive();
        }
    }
    public override void StopActive()
    {
        base.StopActive();

        for (int i = 0; i < logs.Count; i++)
        {
            logs[i].StopActive();
        }

        for (int i = 0; i < gators.Count; i++)
        {
            gators[i].StopActive();
        }
    }
    public void TerrainRiverLane(float moveDised)
    {
        switch (moveDised)
        {
            case -1:
                moveDisLog = Vector2.Distance(startPoint.position, endPoint.position);
                moveDisGator = Vector2.Distance(startPoint.position, endPoint.position);
                break;
            case 0:
                moveDisLog = Vector2.Distance(startPoint.position, endPoint.position);
                moveDisGator = Vector2.Distance(startPoint.position, endPoint.position);
                break;
            case 1:
                moveDisLog = Vector2.Distance(endPoint.position, startPoint.position);
                moveDisGator = Vector2.Distance(endPoint.position, startPoint.position);
                break;
        }
    }
    #region Calculate Log
    public void CalculateUseTimeLog(Log log,float disLog)
    {
        useTimeLog = moveDisLog / rndSpeedLog;

        Debug.Log(log);
        logScript = log.GetComponent<LogScript>();
        logList.Add(log);
        logQueue.Enqueue(log);

        logScript.speed = rndSpeedLog;
        logScript.startPoint = startPoint;
        logScript.endPoint = endPoint;

        foreach (var item in logList)
        {
            item.gameObject.SetActive(false);
        }
        delayTimeLog = useTimeLog / logs.Count;
        if (delayTimeLog < 1f)
        {
            delayTimeLog = 1f;
        }
        InvokeRepeating("LogActive", 0, delayTimeLog);
    }
    public void LogActive()
    {
        if (logQueue.Count == 0)
            return;
        Log logFromQueue = logQueue.Dequeue();
        logFromQueue.gameObject.SetActive(true);
        Invoke("LogReset", useTimeLog);
    }
    public void LogReset()
    {
        logQueue.Enqueue(logList[queueLog]);
        logList[queueLog].transform.position = startPoint.position;
        logList[queueLog].gameObject.SetActive(false);
        queueLog++;
        if (queueLog == logs.Count)
            queueLog = 0;
    }
    #endregion

    #region Calculate Gator
    public void CalculateUseTimeGator(Gator gator, float disGator)
    {
        useTimeGator = moveDisGator / rndSpeedGator;

        logScript = gator.GetComponent<LogScript>();
        gatorsList.Add(gator);
        gatorQueue.Enqueue(gator);

        logScript.speed = rndSpeedGator;
        logScript.startPoint = startPoint;
        logScript.endPoint = endPoint;

        foreach (var item in gatorsList)
        {
            item.gameObject.SetActive(false);
        }
        dalayTimeGator = useTimeGator / gators.Count;
        if (dalayTimeGator < 1f)
        {
            dalayTimeGator = 1f;
        }
        InvokeRepeating("GatorActive", 0, dalayTimeGator);
    }
    public void GatorActive()
    {
        if (gatorQueue.Count == 0)
            return;
        Gator GatorFromQueue = gatorQueue.Dequeue();
        GatorFromQueue.gameObject.SetActive(true);
        Invoke("GatorReset", useTimeGator);
    }
    public void GatorReset()
    {
        gatorQueue.Enqueue(gatorsList[queueGator]);
        gatorsList[queueGator].transform.position = startPoint.position;
        gatorsList[queueGator].gameObject.SetActive(false);
        queueGator++;
        if (queueGator == gators.Count)
            queueGator = 0;
    }
    #endregion
}

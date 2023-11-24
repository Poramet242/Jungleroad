using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLogTerrain : MonoBehaviour
{
    [SerializeField] GameObject log;
    [SerializeField] int logCount;
    [SerializeField] Transform startPoint, endPoint;
    [SerializeField] List<GameObject> logList = new List<GameObject>();
    Queue<GameObject> logQueue = new Queue<GameObject>();
    
    private float rndSpeed, distance;

    [SerializeField] float useTime, delayTime;
    [SerializeField] int queue;

    private void Start()
    {
        rndSpeed = Random.Range(1.5f,3f);
        distance = Vector2.Distance(startPoint.position, endPoint.position);
        Vector2 direction = (endPoint.position - startPoint.position).normalized;
        useTime = distance / rndSpeed;
        for (int i = 0; i < logCount; i++)
        {
            var logSpawn = Instantiate(log, gameObject.transform);
            LogScript logScript = logSpawn.GetComponent<LogScript>();
            logList.Add(logSpawn);
            logQueue.Enqueue(logSpawn);
            logScript.speed = rndSpeed;
            logScript.startPoint = startPoint;
            logScript.endPoint = endPoint;
        }
        foreach (var item in logList)
        {
            item.SetActive(false);
        }
        delayTime = useTime / logCount;
        if(delayTime < 1f)
        {
            delayTime = 1f;
        }
        InvokeRepeating("LogActive", 0, delayTime);
    }
    public void LogActive()
    {
        if (logQueue.Count == 0)
            return;
        GameObject logFromQueue = logQueue.Dequeue();
        logFromQueue.SetActive(true);
        Invoke("LogReset", useTime);
    }
    public void LogReset()
    {
        logQueue.Enqueue(logList[queue]);
        logList[queue].transform.position = startPoint.position;
        logList[queue].SetActive(false);
        queue++;
        if (queue == logCount)
            queue = 0;
    }
}

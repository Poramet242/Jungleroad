using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTerrain : MonoBehaviour
{
    [SerializeField] GameObject car;
    [SerializeField] int carCount;
    [SerializeField] Transform startPoint, endPoint;
    [SerializeField] List<GameObject> carList = new List<GameObject>();
    [SerializeField] float useTime, delayTime;
    [SerializeField] int queue;
    Queue<GameObject> carQueue = new Queue<GameObject>();
    
    private float rndSpeed, distance;
    private void Start()
    {
        rndSpeed = Random.Range(1.5f, 3f);
        distance = Vector2.Distance(startPoint.position, endPoint.position);

        Vector2 direction = (endPoint.position - startPoint.position).normalized;

        useTime = distance / rndSpeed;
        for (int i = 0; i < carCount; i++)
        {
            var carSpawn = Instantiate(car, gameObject.transform);
            CarScript carScript = carSpawn.GetComponent<CarScript>();
            carList.Add(carSpawn);
            carQueue.Enqueue(carSpawn);
            carScript.speed = rndSpeed;
            carScript.startPoint = startPoint;
            carScript.endPoint = endPoint;
        }
        foreach (var item in carList)
        {
            item.SetActive(false);
        }
        delayTime = useTime / carCount;
        if (delayTime < 1f) // maxValueBetweenObject;
        {
            delayTime = 1f;
        }
        InvokeRepeating("CarActive", 0, delayTime);
    }
    public void CarActive()
    {
        if (carQueue.Count == 0)
        {
            return;
        }
        GameObject carFromQueue = carQueue.Dequeue();
        carFromQueue.SetActive(true);
        Invoke("CarReset", useTime);
    }
    public void CarReset()
    {
        carQueue.Enqueue(carList[queue]);
        carList[queue].transform.position = startPoint.position;
        carList[queue].SetActive(false);
        queue++;
        if (queue == carCount)
            queue = 0;
    }
    /*public void SpawnCar(int count)
    {
        carCount = count;
        float rndSpeed = Random.Range(0.3f, 0.5f);
        bool goRight = Random.value >= 0.5;
        for (int i = 0; i < carCount; i++)
        {
            var carSpawn = Instantiate(car, gameObject.transform);
            carList.Add(carSpawn);
        }

    }
    private void OnEnable()
    {
        SpawnCar(3);
    }

    private void OnDisable()
    {
        foreach (var item in carList)
        {
            Destroy(item); // Change to SetActive(false);
        }
        carList.Clear();
    }*/
}

using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using Backend;
using TMPro;

public class TrainLane : Lane
{
    public Animal[] AnimalPrefab;
    public Animal[] AnimalRevertDirectionPrefab;
    public Transform AnimalParent;

    public TMP_Text text;

    private int startIndexPos = -4;
    private List<Animal> trains;
    private List<GameObject> coins;
    private double trainDelay;
    private int trainSize;
    private int moveDirection;
    private double moveSpeed;

    [SerializeField] List<float> TrainDelayTime = new List<float>();
    //---------------------------------------------------------------
    [Header("Edit Attribuild to Dirt Lane")]
    [SerializeField] Transform startPoint, endPoint;
    [SerializeField] float useTime;
    private float rndSpeed;
    private int moveDis;
    TrainScript trainScript;

    [Header("Setup coine in grass lane")]
    public GameObject Coine;
    public Transform CoineParent;
    private JSONArray CoinPosition;

    [Header("Debug GridPivot")]
    [SerializeField]
    private GameObject gridPivot;
    public override void DeActive()
    {
        for(int i = 0; i < trains.Count; i++){
             DestroyImmediate(trains[i]);
        }

        for(int i = 0; i < coins.Count; i++){
            DestroyImmediate(coins[i].gameObject);
        }

        coins.Clear();
        trains.Clear();

        base.DeActive();
    }

    public override void Setup(JSONNode jsonData, int Lindaex)
    {
        base.Setup(jsonData, Lindaex);

        for (int i = 0; i < Floors.Length; i++)
        {
            Floors[i].gameObject.SetActive(true);
            Floors[i].SetType(2);
#if UNITY_EDITOR
            var gridPivotSpawn = Instantiate(gridPivot, Floors[i].transform);
            GridDisplay gd = gridPivotSpawn.GetComponent<GridDisplay>();
            gd.myGridPoint = GridManager.Instance.VectorToGrid(Floors[i].transform.position.x,
            Floors[i].transform.position.y) + new Vector2Int(4, 4);
            gridPivotSpawn.transform.parent = null;
#endif
        }

        trainDelay = jsonData["TrainDelay"].AsFloat;  // delay ของ รถไฟ
        trainSize = jsonData["TrainDelay"].AsInt;  // ขนาดรถไฟรวมหัวขบวน
        moveDirection = jsonData["MoveDirection"].AsInt; //ทิศทาง (-1 วิ่งจากมากไปน้อย, 0 ยืนนิ่ง, 1 วิ่งจากน้อยไปมาก)
        moveSpeed = jsonData["MoveSpeed"].AsDouble; //ความเร็ว
        text.text = jsonData["PatternID"].ToString();
        // TODO จัดการเกี่ยวกับตัวรถไฟ ไม่จำเป็นต้องใช้ prefab กับ class animal ที่ทำไว้ใน demo ลบออกแล้วใช้ class ของจริงได้เลย
        CoinPosition = jsonData["Coin"].AsArray;
        trains = new List<Animal>();
        rndSpeed = (float)moveSpeed;

        JSONObject animalSetting = new JSONObject();
        animalSetting["direction"] = moveDirection;
        animalSetting["speed"] = moveSpeed;
        animalSetting["size"] = trainSize;
        int startAt = Constant.LevelRight;
        if (moveDirection == 1)
        {
            startAt = Constant.LevelLeft;
        }

        //train prefab direction
        var prefab = AnimalPrefab;
        if (moveDirection == -1 && AnimalRevertDirectionPrefab != null) {
            prefab = AnimalRevertDirectionPrefab;
        }
        int TrainIndex = UnityEngine.Random.Range(0,prefab.Length);
        int timeSound = UnityEngine.Random.Range(2, 3);

        Animal train = Instantiate<Animal>(prefab[TrainIndex], Vector3.zero, Quaternion.identity);
        train.transform.parent = AnimalParent;
        train.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(startAt, 0).x, 0, 0);
        train.Setup(animalSetting);
        trains.Add(train);
        TerrainDirtLane(train);

        //Setup Coine
        coins = new List<GameObject>();
        for (int i = 0; i < CoinPosition.Count; i++)
        {
            GameObject coines = Instantiate<GameObject>(Coine);
            coines.transform.parent = CoineParent;
            coines.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).x,
                                                         GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).y, 0);
            coins.Add(coines);
        }
        gameObject.SetActive(true);
    }
    public override void StartActive()
    {
        base.StartActive();

        for(int i = 0; i < trains.Count; i++){
            trains[i].StartActive();
        }
    }
    public override void StopActive()
    {
        base.StopActive();

        for (int i = 0; i < trains.Count; i++)
        {
            trains[i].StopActive();
        }
    }
    public void TerrainDirtLane(Animal train)
    {
        switch (moveDis)
        {
            case -1:
                CalculateUseTime(train, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                TrainDelayTime.Add(Vector2.Distance(train.transform.localPosition, startPoint.localPosition) / rndSpeed);
                //=======================================
                break;
            case 0:
                CalculateUseTime(train, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                TrainDelayTime.Add(Vector2.Distance(train.transform.localPosition, startPoint.localPosition) / rndSpeed);
                break;
            case 1:
                CalculateUseTime(train, Vector2.Distance(startPoint.localPosition, endPoint.localPosition)); // Change position => local
                //================= new ================= R
                TrainDelayTime.Add(Vector2.Distance(endPoint.localPosition, train.transform.position) / rndSpeed);
                //=======================================
                break;
        }
    }
    public void CalculateUseTime(Animal animal, float dist)
    {
        //Debug.Log(Vector2.Distance(startPoint.localPosition, endPoint.localPosition));
        useTime = dist / rndSpeed;
        trainScript = animal.GetComponent<TrainScript>();
        trainScript.startPoint = startPoint;
        trainScript.endPoint = endPoint;
        trainScript.speed = rndSpeed;
        trainScript.delay = (float)trainDelay;
        trainScript.movDir = moveDirection;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using TMPro;
using System.Linq;

public class RiverGatorLane : Lane
{
    public Gator GatorPrefab;
    public Transform MovingBlockParent;
    public TMP_Text text;

    private int moveDirection;
    private double moveSpeed;
    private JSONArray gatorPosition;
    private JSONArray fixGatorDirection;

    private int startIndexPos = -4;
    private List<Gator> gators;
    [SerializeField] private List<GatorScript> gatorSorted;

    [SerializeField] List<float> gatorDelayTime = new List<float>();
    //---------------------------------------------------------------
    [Header("Edit Attribuild to Dirt Lane")]
    [SerializeField] Transform startPoint, endPoint;
    [SerializeField] float useTime;
    private float rndSpeed;
    private int moveDis;
    GatorScript gatorScript;
    public Gator[] GatorDirSprite;
    private int mirrorNum = 0;


    [Header("Debug GridPivot")]
    [SerializeField]
    private GameObject gridPivot;

    public override void DeActive()
    {
        for(int i = 0; i < gators.Count; i++){
            DestroyImmediate(gators[i].gameObject);
        }

        mirrorNum = 0;

        gators.Clear();

        base.DeActive();
    }

    public override void Setup(JSONNode jsonData, int lIndex)
    {
        base.Setup(jsonData, lIndex);

        gatorPosition = jsonData["CrocPosition"].AsArray;   //ตำแหน่งหัวจระเข้
        fixGatorDirection = jsonData["FixCrocDirection"].AsArray;   //ทิศทางหันหน้าของจระเข้ ในกรณีที่ moveDirection = 0 ถ้าไม่ใช่ก็หันทาง moveDirection
        moveDirection = jsonData["MoveDirection"].AsInt;    //ทิศทางการเคลื่อนที่
        moveSpeed = jsonData["MoveSpeed"].AsDouble;     //ความเร็วเคลื่อนที่
        text.text = jsonData["PatternID"].ToString();
        // TODO Spawn จระเข้ตามข้อมูลด้านบน
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
        gators = new List<Gator>();
        gatorSorted = new List<GatorScript>();

        JSONObject gatorSetting = new JSONObject();
        gatorSetting["direction"] = moveDirection;
        gatorSetting["speed"] = moveSpeed;

        rndSpeed = gatorSetting["speed"];
        moveDis = gatorSetting["direction"];

        gatorSetting["size"] = 1;
        //Setup Animal
        if (moveDis == 1)
        {
            GatorPrefab = GatorDirSprite[0];
        }
        else if (moveDis == -1)
        {
            GatorPrefab = GatorDirSprite[1];
        }
        for (int i = 0; i < gatorPosition.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 mirrorPos = new Vector3(
                   GridManager.Instance.GridToVector(((j - 1) * 9), 0).x,
                   GridManager.Instance.GridToVector(((j - 1) * 9), 0).y);

                Gator gator = Instantiate<Gator>(GatorPrefab, Vector3.zero, Quaternion.identity);
                gator.transform.parent = MovingBlockParent;
                gator.transform.localPosition = new Vector3(
                    GridManager.Instance.GridToVector(gatorPosition[i] - 4, 0).x,
                    GridManager.Instance.GridToVector(gatorPosition[i] - 4, 0).y, 
                    0) + mirrorPos;

                gator.Setup(gatorSetting);
                gators.Add(gator);
                TerrainGatorLane(gator);

                gatorScript.gatorPos = (gatorPosition[i] + (j - 1) * 9);
                gatorScript.gatorDelay = gatorDelayTime[mirrorNum];
                gatorScript.useTime = useTime;
                gatorScript.gameObject.name = $"gator{gatorScript.gatorPos}";
                mirrorNum++;
            }
        }

        foreach (var item in gators)
        {
            gatorSorted.Add(item.GetComponent<GatorScript>());
            if (moveDirection == 1)
                gatorSorted = gatorSorted.OrderBy(w => -w.gatorPos).ToList();
            else
                gatorSorted = gatorSorted.OrderBy(w => w.gatorPos).ToList();
        }
        GridDatabase.Instance.gatorDatas.AddRange(gatorSorted);
        gameObject.SetActive(true);
    }

    public override void StartActive()
    {
        base.StartActive();

        for(int i = 0; i < gators.Count; i++){
            gators[i].StartActive();
        }
    }

    public override void StopActive()
    {
        base.StopActive();

        for(int i = 0; i < gators.Count; i++){
            gators[i].StopActive();
        }
    }
    public void TerrainGatorLane(Gator gator)
    {
        switch (moveDis)
        {
            case -1:
                CalculateUseTime(gator, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                gatorDelayTime.Add(Vector2.Distance(gator.transform.localPosition, startPoint.localPosition) / rndSpeed);
                //=======================================
                break;
            case 0:
                CalculateUseTime(gator, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                gatorDelayTime.Add(Vector2.Distance(gator.transform.localPosition, startPoint.localPosition) / rndSpeed);
                //=======================================
                break;
            case 1:
                CalculateUseTime(gator, Vector2.Distance(startPoint.localPosition, endPoint.localPosition)); // Change position => local
                //================= new ================= R
                gatorDelayTime.Add(Vector2.Distance(endPoint.localPosition, gator.transform.localPosition) / rndSpeed);
                //=======================================
                break;
        }
    }
    public void CalculateUseTime(Gator gator, float dist)
    {
        useTime = dist / rndSpeed;
        gatorScript = gator.GetComponent<GatorScript>();
        gatorScript.startPoint = startPoint;
        gatorScript.endPoint = endPoint;
        gatorScript.speed = rndSpeed;
        gatorScript.dis = moveDis;
    }

}

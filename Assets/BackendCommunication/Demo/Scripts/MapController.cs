using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using Backend;

public class MapController : MonoBehaviour
{
    public Transform LaneParent;
    public MapPooling mapPool;
    
    public TextAsset fixMapJsonFile;

    private List<Lane> spawnLanes;

    private int startY = -4;
    private int currentMapSpawnIndex = 0;
    private bool isStarted = false;

    void Start()
    {
        const string logTag = "[MapController::Start]";
        // NOTE: chanyutl - 
        // we try to get seed as the first approach for generating the map
        // otherwise if seed is not available, we consider the fixMapJSON file for the second approach
        // however if both of them is unavailable, we consider this situation as a failure case
        spawnLanes = new List<Lane>();

        uint? seed = null;
        try
        {
            var seed_ = DataManager.Instance.nextSeed.GetOnce();
            seed = seed_;
        }
        catch (Exception)
        {
            seed = null;
        }

        if (fixMapJsonFile != null)
        {
            JSONNode jsonData = JSON.Parse(fixMapJsonFile.text);
            SetMap(jsonData["map"].AsArray);
        }
        else if (seed.HasValue)
        {
            Debug.Log($"Seed: {seed.Value}");
            MapGenerator.instance.SetSeed(seed.Value);
            GenerateRepeat(50);
            //GenerateRepeat(50);
            //GenerateRepeat(50);
        }
        else
        {
            throw new System.InvalidOperationException("no valid way to generate the map");
        }

        Backend.BackendComm.instance.SendClientReady(gameTime: 0);
        Debug.Log($"{logTag} sent clientReady packet");
    }

    void Update()
    {
        //NODE: chanyutl - please cover the code snippet for debugging/testing with directive UNITY_EDITOR
        // to prevent unintentionally secuity mistake in the build
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!isStarted) StartGame();
        }

        if (Input.GetKeyDown(KeyCode.G)){
            GenerateRepeat(50);
        }

        if (Input.GetKeyDown(KeyCode.R)){
            DeSpawnLane(10);
        }
#endif
    }

    public void SetMap(JSONArray mapData)
    {
        //spawnLanes.Clear();

        Debug.Log($"[MapController] SetMap with {mapData.Count} lanes");
        AddMap(mapData);
    }

    public void AddMap(JSONArray mapData){
        for (int i = 0; i < mapData.Count; i++)
        {
            JSONNode laneData = mapData[i];

            Lane lane = mapPool.GetLane(laneData["type"].AsInt);
            if (lane != null)
            {
                lane.transform.SetParent(LaneParent);
                lane.transform.localPosition = GridManager.Instance.GridToVector(0, startY + currentMapSpawnIndex);
                lane.Setup(laneData, currentMapSpawnIndex);
                spawnLanes.Add(lane);

                if ((currentMapSpawnIndex != 0) && (currentMapSpawnIndex % 10 == 0))
                {
                    ScoreManager.Instance.CheckPoint(currentMapSpawnIndex);
                }

                currentMapSpawnIndex++;
            }
        }
    }

    public void DeSpawnLane(int count){
        Debug.Log($"[DeSpawnLane] Attempting to despawn {count} lanes, currently have {spawnLanes.Count}");
        for(int i = 0; i < count; i++){
            spawnLanes[i].DeActive();
        }

        spawnLanes.RemoveRange(0, count);
        Debug.Log($"[DeSpawnLane] despawned {count} lanes, currently have {spawnLanes.Count}");
    }

    public void GenerateRepeat(int valuemap)
    {
        Debug.Log($"[GenerateRepeat] {valuemap} lanes");
        JSONArray jArr = new JSONArray();
        MapGenerator.instance.GenerateLevel(valuemap).ForEach(n => jArr.Add(n));
        AddMap(jArr);
    }

    public void StartGame()
    {
        mapPool.StartActiveLane();
        isStarted = true;
    }

    public void StopGame()
    {
        mapPool.StopActiveLane();
        isStarted = false;
    }
}
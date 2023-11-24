using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Backend;
using System;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;
    public TextAsset GrassLaneTemplate;
    public TextAsset DirtLaneTemplate;
    public TextAsset RiverLaneTemplate;
    public TextAsset FallbackLaneTemplate;

    private JSONNode grassLaneData;
    private JSONNode dirtLaneData;
    private JSONNode riverLaneData;
    private JSONNode fallbackLaneData;

    private RandomNumberGenerator randomNumber;

    private List<JSONNode> levelGenerated;
    private int[] availableLaneType;
    private int currentLaneIndex;
    private int previousLaneType;
    private string previousTemplateIndex;
    private int previousCoinSpawnGroup;
    private JSONArray previousExit;

    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
        }
        instance = this;

        loadTemplateData();
        currentLaneIndex = 0;
        levelGenerated = new List<JSONNode>();
    }

    void Start()
    {
        // RandomNumberGenerator testRand = new RandomNumberGenerator(1111);

        // JSONArray rIdexCol = new JSONArray();
        // for(int i = 0; i < 30; i++){
        //     int testRandValue = testRand.RandomIndex(5);
        //     rIdexCol[i] = testRandValue;
        // }

        // Debug.Log($"random index: {rIdexCol.ToString()}");
        // SetSeed(12345);
        // List<JSONNode> demoLevel = GenerateLevel(50);
        // JSONArray demoArray = new JSONArray();
        // for(int i = 0; i < demoLevel.Count; i++){
        //     demoArray.Add(demoLevel[i]);
        // }
        // Debug.Log(demoArray.ToString());
    }
    public void RandomGeneratorNewmap(int map)
    {
        List<JSONNode> demoLevel = GenerateLevel(map);
    }
    void loadTemplateData()
    {
        availableLaneType = new int[] {1, 2, 4};

        if (GrassLaneTemplate != null)
        {
            grassLaneData = JSON.Parse(GrassLaneTemplate.text);
        }
        if (DirtLaneTemplate != null)
        {
            dirtLaneData = JSON.Parse(DirtLaneTemplate.text);
        }
        if (RiverLaneTemplate != null)
        {
            riverLaneData = JSON.Parse(RiverLaneTemplate.text);
        }
        if (FallbackLaneTemplate != null)
        {
            fallbackLaneData = JSON.Parse(FallbackLaneTemplate.text);
        }
    }

    public void SetSeed(uint seed)
    {
        Debug.Log($"[MapGenerator] SetSeed = {seed}");
        randomNumber = new RandomNumberGenerator(seed);
    }

    public List<JSONNode> GetAllLevel()
    {
        return levelGenerated;
    }

    // public List<JSONNode> LoadFixLevel()
    // {
    //     if (FixLevelFile == null)
    //     {
    //         Debug.LogError("FixLevelFils is missing");
    //         return null;
    //     }

    //     List<JSONNode> levels = new List<JSONNode>();
    //     JSONNode fixMapJson = JSON.Parse(FixLevelFile.text);
    //     JSONArray lanes = fixMapJson["map"].AsArray;

    //     for (int i = 0; i < lanes.Count; i++)
    //     {
    //         JSONNode lane = lanes[i].AsObject;
    //         int laneType = lane["type"].AsInt;
    //         JSONObject newLane = null;
    //         switch ((Constant.LaneTypeEnum)laneType)
    //         {
    //             case Constant.LaneTypeEnum.grassLane:
    //                 newLane = new JSONObject();
    //                 newLane["PatternID"] = lane["PatternID"].Value;
    //                 newLane["type"] = laneType;
    //                 newLane["TreePosition"] = lane["TreePosition"].AsArray;
    //                 newLane["Coin"] = lane["Coin"].AsArray;
    //                 break;
    //             case Constant.LaneTypeEnum.dirtLane:
    //                 newLane = new JSONObject();
    //                 newLane["PatternID"] = lane["PatternID"].Value;
    //                 newLane["type"] = laneType;
    //                 newLane["AnimalPosition"] = lane["AnimalPosition"].AsArray;
    //                 newLane["MoveDirection"] = lane["MoveDirection"].AsInt;
    //                 newLane["MoveSpeed"] = lane["MoveSpeed"].AsFloat;
    //                 newLane["Coin"] = lane["Coin"].AsArray;
    //                 break;
    //             case Constant.LaneTypeEnum.bigDirtLane:
    //                 newLane = new JSONObject();
    //                 newLane["PatternID"] = lane["PatternID"].Value;
    //                 newLane["type"] = laneType;
    //                 newLane["TrainDelay"] = lane["TrainDelay"].AsArray;
    //                 newLane["TrainSize"] = lane["TrainSize"].AsArray;
    //                 newLane["MoveDirection"] = lane["MoveDirection"].AsInt;
    //                 newLane["MoveSpeed"] = lane["MoveSpeed"].AsFloat;
    //                 newLane["Coin"] = lane["Coin"].AsArray;
    //                 break;
    //             case Constant.LaneTypeEnum.riverLane:
    //                 newLane = new JSONObject();
    //                 newLane["PatternID"] = lane["PatternID"].Value;
    //                 newLane["type"] = laneType;
    //                 newLane["LotusPosition"] = lane["LotusPosition"].AsArray;
    //                 newLane["MoveDirection"] = lane["MoveDirection"].AsInt;
    //                 newLane["MoveSpeed"] = lane["MoveSpeed"].AsFloat;
    //                 newLane["Coin"] = lane["Coin"].AsArray;
    //                 break;
    //             case Constant.LaneTypeEnum.riverLogLane:
    //                 newLane = new JSONObject();
    //                 newLane["PatternID"] = lane["PatternID"].Value;
    //                 newLane["type"] = laneType;
    //                 newLane["LogPosition"] = lane["LogPosition"].AsArray;
    //                 newLane["MoveDirection"] = lane["MoveDirection"].AsInt;
    //                 newLane["MoveSpeed"] = lane["MoveSpeed"].AsFloat;
    //                 break;
    //             case Constant.LaneTypeEnum.riverGatorLane:
    //                 newLane = new JSONObject();
    //                 newLane["PatternID"] = lane["PatternID"].Value;
    //                 newLane["type"] = laneType;
    //                 newLane["CrocPosition"] = lane["CrocPosition"].AsArray;
    //                 newLane["FixCrocDirection"] = lane["FixCrocDirection"].AsArray;
    //                 newLane["MoveDirection"] = lane["MoveDirection"].AsInt;
    //                 newLane["MoveSpeed"] = lane["MoveSpeed"].AsFloat;
    //                 break;
    //         }
    //         if (newLane != null)
    //         {
    //             levels.Add(newLane);
    //         }
    //     }

    //     return levels;
    // }

    public List<JSONNode> GenerateLevel(int totalLane)
    {
        Debug.Log($"[MapGenerator] GenerateLevel {totalLane} lanes");
        List<JSONNode> levels = new List<JSONNode>();

        List<JSONObject> generateLanes = null;
        int pickedLaneType = 0;
        if (currentLaneIndex == 0)
        {
            List<JSONNode> startZone = generateStartZone();
            levels.AddRange(startZone);
            totalLane -= startZone.Count;
        }

        while (totalLane > 0)
        {
            pickedLaneType = pickRandomLaneType(null);
            if (generateLanes != null)
            {
                generateLanes.Clear();
            }

            switch ((Constant.LaneTypeEnum)pickedLaneType)
            {
                case Constant.LaneTypeEnum.grassLane:
                    generateLanes = generateGrassLaneFromArchive();
                    break;
                case Constant.LaneTypeEnum.dirtLane:
                    generateLanes = generateDirtLaneFromArchive();
                    break;
                case Constant.LaneTypeEnum.riverLane:
                    generateLanes = generateRiverLaneFromArchive();
                    break;
            }

            levels.AddRange(generateLanes);
            totalLane -= generateLanes.Count;
        }

        levelGenerated.AddRange(levels);

        return levels;
    }

    public List<JSONNode> generateStartZone()
    {
        List<JSONNode> levels = new List<JSONNode>();

        int startZoneLaneCount = Constant.PreStartZoneLaneCount + Constant.StartZoneLaneCount;

        if (currentLaneIndex == 0)
        {
            for (int i = 0; i < startZoneLaneCount; i++)
            {
                JSONObject startLane = new JSONObject();
                startLane["LaneNumber"] = i+1;
                startLane["PatternID"] = "0";
                startLane["type"] = (int)Constant.LaneTypeEnum.grassLane;
                startLane["TreePosition"] = new JSONArray();

                currentLaneIndex++;

                previousLaneType = (int)Constant.LaneTypeEnum.grassLane;

                levels.Add(startLane);
            }

            previousExit = new JSONArray();
            previousExit[0] = 0;
            previousExit[1] = 1;
            previousExit[2] = 2;
            previousExit[3] = 3;
            previousExit[4] = 4;
            previousExit[5] = 5;
            previousExit[6] = 6;
            previousExit[7] = 7;
            previousExit[8] = 8;
        }

        List<JSONObject> generateLanes = null;
        if (currentLaneIndex == startZoneLaneCount)
        {
            generateLanes = generateGrassLaneFromArchive();
            previousLaneType = (int)Constant.LaneTypeEnum.grassLane;
            levels.AddRange(generateLanes);
        }

        int pickedLaneType;
        int[] noRiver = new int[] {4};
        while(currentLaneIndex < Constant.MapGen_FixNoRiverUntil) {
            pickedLaneType = pickRandomLaneType(noRiver);
            if (generateLanes != null)
            {
                generateLanes.Clear();
            }

            switch ((Constant.LaneTypeEnum)pickedLaneType)
            {
                case Constant.LaneTypeEnum.grassLane:
                    generateLanes = generateGrassLaneFromArchive();
                    break;
                case Constant.LaneTypeEnum.dirtLane:
                    generateLanes = generateDirtLaneFromArchive();
                    break;
                case Constant.LaneTypeEnum.riverLane:
                    generateLanes = generateRiverLaneFromArchive();
                    break;
            }

            levels.AddRange(generateLanes);
        }

        return levels;
    }

    #region random
    private string pickRandomIds(string[] idsPool, int laneType)
    {
        List<string> validIdPool = new List<string> { };
        if (laneType == previousLaneType)
        {
            for (int i = 0; i < idsPool.Length; i++)
            {
                if (idsPool[i].Equals(previousTemplateIndex))
                {
                    continue;
                }
                validIdPool.Add(idsPool[i]);
            }
        }
        else
        {
            for (int i = 0; i < idsPool.Length; i++)
            {
                validIdPool.Add(idsPool[i]);
            }
        }

        int randomIndex = randomNumber.RandomIndex(validIdPool.Count);
        previousTemplateIndex = validIdPool[randomIndex];

        return validIdPool[randomIndex];
    }

    private int pickRandomLaneType(int[] exceptLaneType)
    {
        List<int> validType = new List<int>();
        bool valid = true;
        for (int i = 0; i < availableLaneType.Length; i++)
        {

            valid = true;
            if (availableLaneType[i] == previousLaneType)
            {
                valid = false;
                continue;
            }

            if(exceptLaneType != null) {
                for(int j = 0; j < exceptLaneType.Length; j++){
                    if (availableLaneType[i] == exceptLaneType[j])
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if(valid){
                validType.Add(availableLaneType[i]);
            }
        }

        int randomIndex = randomNumber.RandomIndex(validType.Count);
        previousLaneType = validType[randomIndex];
        return validType[randomIndex];
    }

    private bool randomCoinSpawn()
    {
        bool spawnCoin = false;
        int currentCoinGroup = currentLaneIndex / Constant.CoinSpawnPeriod;
        if (currentCoinGroup > previousCoinSpawnGroup)
        {
            //enter new coin period
            double rV = randomNumber.RandomValue();
            if (rV <= Constant.CoinSpawnPercentage)
            {
                spawnCoin = true;
                previousCoinSpawnGroup = currentCoinGroup;
            }
        }

        return spawnCoin;
    }

    private JSONNode pickRandomLaneTemplate(JSONNode templateDatas, int laneType)
    {
        JSONNode laneData;
        JSONArray laneEntry;
        int connectPoint = 0;
        List<string> idsPool = new List<string>();

        string key;
        for (int i = 1; i <= templateDatas.Count; i++)
        {
            key = i.ToString();
            laneData = templateDatas[key];
            if (laneData != null)
            {
                laneEntry = laneData["entry"].AsArray;
                connectPoint = 0;

                for (int j = 0; j < laneEntry.Count; j++)
                {
                    for (int k = 0; k < previousExit.Count; k++)
                    {
                        if (laneEntry[j].AsInt == previousExit[k].AsInt)
                        {
                            connectPoint += 1;
                            break;
                        }
                    }
                }

                if (connectPoint < Constant.MapGen_MinConnectedPoint)
                {
                    continue;
                }

                idsPool.Add(laneData["pattern_id"].Value);
            }
        }

        if (idsPool.Count == 0)
        {
            previousExit = fallbackLaneData["1"]["exit"].AsArray;
            return fallbackLaneData["1"];
        }

        string picked = pickRandomIds(idsPool.ToArray(), laneType);

        laneData = templateDatas[picked];
        previousExit = laneData["exit"].AsArray;

        return laneData;
    }

    private double pickRandomSpeed(int currentIndex, bool isTrain){
        //TODO update random speed for train
        if(isTrain){
            return Constant.TrainSpeed_Min + (randomNumber.RandomValue() * Constant.TrainSpeed_Max);
        }

        return Constant.LaneSpeed_Min + (randomNumber.RandomValue() * Constant.LaneSpeed_Max);
    }

    #endregion

    #region level generate
    private List<JSONObject> generateGrassLaneFromArchive()
    {
        return generateLanesFromArchive(Constant.LaneTypeEnum.grassLane, grassLaneData);
    }

    private List<JSONObject> generateDirtLaneFromArchive()
    {
        return generateLanesFromArchive(Constant.LaneTypeEnum.dirtLane, dirtLaneData);
    }

    private List<JSONObject> generateRiverLaneFromArchive()
    {
        return generateLanesFromArchive(Constant.LaneTypeEnum.riverLane, riverLaneData);
    }

    private List<JSONObject> generateLanesFromArchive(Constant.LaneTypeEnum laneType, JSONNode templateDatas)
    {
        List<JSONObject> lanes = new List<JSONObject>();
        JSONNode templateData = pickRandomLaneTemplate(templateDatas, (int)laneType);
        JSONArray laneData = templateData["lane"].AsArray;

        for (int i = 0; i < laneData.Count; i++)
        {
            int spawnLaneIndex = currentLaneIndex + 1;
            Constant.LaneTypeEnum generateLaneType = (Constant.LaneTypeEnum)laneData[i]["type"].AsInt;
            JSONObject lane = null;
            switch (generateLaneType)
            {
                case Constant.LaneTypeEnum.grassLane:
                    lane = newGrassLane(spawnLaneIndex, laneData[i], templateData["pattern_id"].Value);
                    break;
                case Constant.LaneTypeEnum.dirtLane:
                case Constant.LaneTypeEnum.bigDirtLane:
                    lane = newDirtLane(spawnLaneIndex, laneData[i], templateData["pattern_id"].Value);
                    break;
                case Constant.LaneTypeEnum.riverLane:
                case Constant.LaneTypeEnum.riverLogLane:
                case Constant.LaneTypeEnum.riverGatorLane:
                    lane = newRiverLane(spawnLaneIndex, laneData[i], templateData["pattern_id"].Value);
                    break;
            }

            if (lane != null)
            {
                lanes.Add(lane);
                currentLaneIndex = spawnLaneIndex;
            }
        }

        return lanes;
    }

    private JSONObject newGrassLane(int laneNumber, JSONNode rawLaneData, string patternID)
    {
        JSONObject lane = new JSONObject();
        lane["PatternID"] = patternID;
        lane["LaneNumber"] = laneNumber;
        lane["type"] = rawLaneData["type"].AsInt;
        lane["TreePosition"] = rawLaneData["TreePosition"].AsArray;



        if (randomCoinSpawn())
        {
            List<int> validCoinBlock = new List<int>();
            for (int i = Constant.LeftMostLaneIndex; i <= Constant.RightMostLaneIndex; i++)
            {
                bool valid = true;
                for (int j = 0; j < lane["TreePosition"].Count; j++)
                {
                    if (i == lane["TreePosition"][j].AsInt)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    validCoinBlock.Add(i);
                }
            }

            if (validCoinBlock.Count > 0)
            {
                int spawnCoinInValidIndex = randomNumber.RandomIndex(validCoinBlock.Count);
                lane["Coin"] = new JSONArray();
                lane["Coin"][0] = validCoinBlock[spawnCoinInValidIndex];
            }
            else
            {
                lane["Coin"] = new JSONArray();
            }
        }
        else
        {
            lane["Coin"] = new JSONArray();
        }

        return lane;
    }

    private JSONObject newDirtLane(int laneNumber, JSONNode rawLaneData, string patternID)
    {
        JSONObject lane = new JSONObject();
        lane["PatternID"] = patternID;
        lane["LaneNumber"] = laneNumber;
        lane["type"] = rawLaneData["type"].AsInt;

        int randomPickDirection = randomNumber.RandomIndex(rawLaneData["MoveDirection"].AsArray.Count);
        lane["MoveDirection"] = rawLaneData["MoveDirection"].AsArray[randomPickDirection].AsInt;
        
        if (lane["type"].AsInt == (int)Constant.LaneTypeEnum.dirtLane)
        {
            lane["MoveSpeed"] = pickRandomSpeed(currentLaneIndex, false);
            lane["AnimalPosition"] = rawLaneData["AnimalPosition"].AsArray;
        }
        else if (lane["type"].AsInt == (int)Constant.LaneTypeEnum.bigDirtLane)
        {
            lane["MoveSpeed"] = pickRandomSpeed(currentLaneIndex, true);
            lane["TrainDelay"] = rawLaneData["TrainDelay"].AsFloat;
            lane["TrainSize"] = rawLaneData["TrainSize"].AsInt;
        }

        lane["Coin"] = new JSONArray();

        return lane;
    }

    private JSONObject newRiverLane(int laneNumber, JSONNode rawLaneData, string patternID)
    {
        JSONObject lane = new JSONObject();
        lane["PatternID"] = patternID;
        lane["LaneNumber"] = laneNumber;
        lane["type"] = rawLaneData["type"].AsInt;


        if (lane["type"].AsInt == (int)Constant.LaneTypeEnum.riverLane)
        {
            lane["LotusPosition"] = rawLaneData["LotusPosition"].AsArray;
        }
        else if (lane["type"].AsInt == (int)Constant.LaneTypeEnum.riverLogLane)
        {
            lane["LogPosition"] = rawLaneData["LogPosition"].AsArray;
            int randomPickDirection = randomNumber.RandomIndex(rawLaneData["MoveDirection"].AsArray.Count);
            lane["MoveDirection"] = rawLaneData["MoveDirection"].AsArray[randomPickDirection].AsInt;
            lane["MoveSpeed"] = pickRandomSpeed(currentLaneIndex, false);
        }
        else if (lane["type"].AsInt == (int)Constant.LaneTypeEnum.riverGatorLane)
        {
            lane["CrocPosition"] = rawLaneData["CrocPosition"].AsArray;
            lane["FixCrocDirection"] = rawLaneData["FixCrocDirection"].AsArray;
            int randomPickDirection = randomNumber.RandomIndex(rawLaneData["MoveDirection"].AsArray.Count);
            lane["MoveDirection"] = rawLaneData["MoveDirection"].AsArray[randomPickDirection].AsInt;
            lane["MoveSpeed"] = pickRandomSpeed(currentLaneIndex, false);
        }

        List<int> validCoinBlock = new List<int>();

        if ((lane["type"].AsInt == (int)Constant.LaneTypeEnum.riverLane) && (lane["LotusPosition"].Count > 0))
        {
            if (randomCoinSpawn())
            {
                for (int i = Constant.LeftMostLaneIndex; i <= Constant.RightMostLaneIndex; i++)
                {
                    bool valid = false;
                    for (int j = 0; j < lane["LotusPosition"].Count; j++)
                    {
                        if (i == lane["LotusPosition"][j].AsInt)
                        {
                            valid = true;
                            break;
                        }
                    }
                    if (valid)
                    {
                        validCoinBlock.Add(i);
                    }
                }
            }
        }

        if (validCoinBlock.Count > 0)
        {
            string coinPoint = "[ ";
            for (int i = 0; i < validCoinBlock.Count; i++)
            {
                coinPoint += validCoinBlock[i].ToString() + " ";
            }
            coinPoint += "]";
            int spawnCoinInValidIndex = randomNumber.RandomIndex(validCoinBlock.Count);
            lane["Coin"] = new JSONArray();
            lane["Coin"][0] = validCoinBlock[spawnCoinInValidIndex];
        }
        else
        {
            lane["Coin"] = new JSONArray();
        }

        return lane;
    }
    #endregion
}

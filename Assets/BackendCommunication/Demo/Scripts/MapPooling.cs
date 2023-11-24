using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using Backend;

public class MapPooling : MonoBehaviour
{
    public Lane GrassLanePrefab;
    public int GrassLaneAmountToPool;
    public Lane DirtLanePrefab;
    public int DirtLaneAmountToPool;
    public Lane TrainLanePrefab;
    public int TrainLaneAmountToPool;
    public Lane RiverLotusLanePrefab;
    public int RiverLotusLaneAmountToPool;
    public Lane RiverLogLanePrefab;
    public int RiverLogLaneAmountToPool;
    public Lane RiverGatorLanePrefab;
    public int RiverGatorLaneAmountToPool;

    private List<Lane> grassLanePool;
    private List<Lane> dirtLanePool;
    private List<Lane> trainLanePool;
    private List<Lane> lotusLanePool;
    private List<Lane> logLanePool;
    private List<Lane> gatorLanePool;
    private List<Lane> lanes;

    void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        lanes = new List<Lane>();

        grassLanePool = new List<Lane>();
        for (int i = 0; i < GrassLaneAmountToPool; i++)
        {
            AddLaneToPool(Constant.LaneTypeEnum.grassLane);
        }

        dirtLanePool = new List<Lane>();
        for (int i = 0; i < DirtLaneAmountToPool; i++)
        {
            AddLaneToPool(Constant.LaneTypeEnum.dirtLane);
        }

        trainLanePool = new List<Lane>();
        for (int i = 0; i < TrainLaneAmountToPool; i++)
        {
            AddLaneToPool(Constant.LaneTypeEnum.bigDirtLane);
        }

        lotusLanePool = new List<Lane>();
        for (int i = 0; i < RiverLotusLaneAmountToPool; i++)
        {
            AddLaneToPool(Constant.LaneTypeEnum.riverLane);
        }

        logLanePool = new List<Lane>();
        for (int i = 0; i < RiverLogLaneAmountToPool; i++)
        {
            AddLaneToPool(Constant.LaneTypeEnum.riverLogLane);
        }

        gatorLanePool = new List<Lane>();
        for (int i = 0; i < RiverGatorLaneAmountToPool; i++)
        {
            AddLaneToPool(Constant.LaneTypeEnum.riverGatorLane);
        }
    }

    public Lane GetLane(int laneType)
    {
        switch ((Constant.LaneTypeEnum)laneType)
        {
            case Constant.LaneTypeEnum.grassLane:
                for (int i = 0; i < GrassLaneAmountToPool; i++)
                {
                    if (!grassLanePool[i].gameObject.activeInHierarchy)
                    {
                        return grassLanePool[i];
                    }
                }

                GrassLaneAmountToPool++;
                return AddLaneToPool(Constant.LaneTypeEnum.grassLane);
            case Constant.LaneTypeEnum.dirtLane:
                for (int i = 0; i < DirtLaneAmountToPool; i++)
                {
                    if (!dirtLanePool[i].gameObject.activeInHierarchy)
                    {
                        return dirtLanePool[i];
                    }
                }
                DirtLaneAmountToPool++;
                return AddLaneToPool(Constant.LaneTypeEnum.dirtLane);
            case Constant.LaneTypeEnum.bigDirtLane:
                for (int i = 0; i < TrainLaneAmountToPool; i++)
                {
                    if (!trainLanePool[i].gameObject.activeInHierarchy)
                    {
                        return trainLanePool[i];
                    }
                }
                TrainLaneAmountToPool++;
                return AddLaneToPool(Constant.LaneTypeEnum.bigDirtLane);
            case Constant.LaneTypeEnum.riverLane:
                for (int i = 0; i < RiverLotusLaneAmountToPool; i++)
                {
                    if (!lotusLanePool[i].gameObject.activeInHierarchy)
                    {
                        return lotusLanePool[i];
                    }
                }
                RiverLotusLaneAmountToPool++;
                return AddLaneToPool(Constant.LaneTypeEnum.riverLane);
            case Constant.LaneTypeEnum.riverLogLane:
                for (int i = 0; i < RiverLogLaneAmountToPool; i++)
                {
                    if (!logLanePool[i].gameObject.activeInHierarchy)
                    {
                        return logLanePool[i];
                    }
                }
                RiverLogLaneAmountToPool++;
                return AddLaneToPool(Constant.LaneTypeEnum.riverLogLane);
            case Constant.LaneTypeEnum.riverGatorLane:
                for (int i = 0; i < RiverGatorLaneAmountToPool; i++)
                {
                    if (!gatorLanePool[i].gameObject.activeInHierarchy)
                    {
                        return gatorLanePool[i];
                    }
                }
                RiverGatorLaneAmountToPool++;
                return AddLaneToPool(Constant.LaneTypeEnum.riverGatorLane);
        }
        return null;
    }

    private Lane AddLaneToPool(Constant.LaneTypeEnum laneType)
    {
        Lane lane = null;
        switch (laneType)
        {
            case Constant.LaneTypeEnum.grassLane:
                lane = Instantiate<Lane>(GrassLanePrefab, Vector3.zero, Quaternion.identity);
                break;
            case Constant.LaneTypeEnum.dirtLane:
                lane = Instantiate<Lane>(DirtLanePrefab, Vector3.zero, Quaternion.identity);
                break;
            case Constant.LaneTypeEnum.bigDirtLane:
                lane = Instantiate<Lane>(TrainLanePrefab, Vector3.zero, Quaternion.identity);
                break;
            case Constant.LaneTypeEnum.riverLane:
                lane = Instantiate<Lane>(RiverLotusLanePrefab, Vector3.zero, Quaternion.identity);
                break;
            case Constant.LaneTypeEnum.riverLogLane:
                lane = Instantiate<Lane>(RiverLogLanePrefab, Vector3.zero, Quaternion.identity);
                break;
            case Constant.LaneTypeEnum.riverGatorLane:
                lane = Instantiate<Lane>(RiverGatorLanePrefab, Vector3.zero, Quaternion.identity);
                break;
        }

        lane.gameObject.SetActive(false);

        switch (laneType)
        {
            case Constant.LaneTypeEnum.grassLane:
                grassLanePool.Add(lane);
                break;
            case Constant.LaneTypeEnum.dirtLane:
                dirtLanePool.Add(lane);
                break;
            case Constant.LaneTypeEnum.bigDirtLane:
                trainLanePool.Add(lane);
                break;
            case Constant.LaneTypeEnum.riverLane:
                lotusLanePool.Add(lane);
                break;
            case Constant.LaneTypeEnum.riverLogLane:
                logLanePool.Add(lane);
                break;
            case Constant.LaneTypeEnum.riverGatorLane:
                gatorLanePool.Add(lane);
                break;
        }
        lanes.Add(lane);

        return lane;
    }
   
    public void StartActiveLane()
    {
        for (int i = 0; i < lanes.Count; i++)
        {
            if (lanes[i].gameObject.activeInHierarchy)
            {
                lanes[i].StartActive();
            }

        }
    }
    
    public void StopActiveLane()
    {
        for (int i = 0; i < lanes.Count; i++)
        {
            if (lanes[i].gameObject.activeInHierarchy)
            {
                lanes[i].StopActive();
            }

        }
    }
}
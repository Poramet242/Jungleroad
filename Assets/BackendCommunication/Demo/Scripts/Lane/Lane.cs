using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;
using Backend;

public abstract class Lane : MonoBehaviour
{
    public enum LaneTypeEnum {
        grassLane = 1,
        dirtLane = 2,
        bigDirtLane = 3,
        riverLane = 4,
        riverLogLane = 5,
        riverGatorLane = 6
    }
    public Floor[] Floors;
    public int LaneIndex;
    public LaneTypeEnum LaneType;
    public DateTime ActiveAt;
    public DateTime CreateAt;
    private JSONArray coinPosition;

    public virtual void DeActive(){
        gameObject.SetActive(false);
    }

    public virtual void Setup(JSONNode jsonData, int lIndex){
        LaneType = (LaneTypeEnum)jsonData["type"].AsInt;
        coinPosition = jsonData["Coin"].AsArray;
        CreateAt = DateTime.Now;
        LaneIndex = lIndex;

        //TODO จัดการ spawn เหรียญตามตำแหน่งใน coinPosition
    }

    public virtual void StartActive(){
        ActiveAt = DateTime.Now;
    }

    public virtual void StopActive(){}
}

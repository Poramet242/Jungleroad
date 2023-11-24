using System;

using UnityEngine;

namespace Backend
{
    public class ActionResponsePackage
    {
        public byte PackageType;
        public int PackageNumber;
        public double ServerGameTime;
        public int ActionSeqNumber;
        public int CurrentLane;
        public int CurrentIndex;
        public bool CanMove;
        public byte MoveResult;
        public bool CollectCoin;

        public void FromByte(byte[] data){
            PackageType = data[0];
            ServerGameTime = BitConverter.ToDouble(data, 1);
            
            PackageNumber = BitConverter.ToInt32(data, 9);
            ActionSeqNumber = BitConverter.ToInt32(data, 13);
            CurrentLane = BitConverter.ToInt32(data, 17);
            CurrentIndex = BitConverter.ToInt32(data, 21);
            CanMove = data[25] == 1;
            MoveResult = data[26];
            CollectCoin = data[27] == 1;
        }

        public override string ToString()
        {
            return "[ActionResult] PlayerLocation: ["+CurrentLane+":"+CurrentIndex+"] CanMove: "+CanMove+" MoveResult: "+MoveResult+" Collect Coin: "+CollectCoin;
        }

        public string HumanReadable() => $"[ActionResult] PlayerLocation: [{CurrentLane}:{CurrentIndex}] CanMove: {CanMove} MoveResult: {MoveResult} Collect Coin: {CollectCoin}";

        
    }
}
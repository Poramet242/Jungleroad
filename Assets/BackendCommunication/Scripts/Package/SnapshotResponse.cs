using System;
using UnityEngine;

namespace Backend
{
    public class SnapshotResponsePackage
    {
        public byte PackageType;
        public int PackageNumber;
        public double ClientGameTime;
        public int CurrentLane;
        public int CurrentIndex;
        public byte PlayerStatus;

        public void FromByte(byte[] data){
            Debug.Log($"[SnapResponse] FromByte");
            PackageType = data[0];
            ClientGameTime = BitConverter.ToDouble(data, 1);
            PackageNumber = BitConverter.ToInt32(data, 9);
            CurrentLane = BitConverter.ToInt32(data, 13);
            CurrentIndex = BitConverter.ToInt32(data, 17);
            PlayerStatus = data[21];
        }
    }
}
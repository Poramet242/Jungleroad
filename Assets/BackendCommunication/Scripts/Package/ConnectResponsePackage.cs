using System;
using UnityEngine;

namespace Backend
{
    public class ConnectResponsePackage
    {
        public byte PackageType;
        public int PackageNumber;
        public double ServerGameTime;
        public uint Seed;
        public bool CanPlay;
        public byte ErrStatus;

        public void FromByte(byte[] data){
            Debug.Log($"[ConnectResponse] FromByte");
            PackageType = data[0];
            Debug.Log($"[ConnectResponse] ServerGameTime");
            ServerGameTime = BitConverter.ToDouble(data, 1);
            Debug.Log($"[ConnectResponse] PackageNumber");
            PackageNumber = BitConverter.ToInt32(data, 9);
            Debug.Log($"[ConnectResponse] Seed");
            Seed = BitConverter.ToUInt32(data, 13);
            CanPlay = data[17] == 1;
            ErrStatus = data[18];
        }

        public override string ToString()
        {
            return "[ConnectResponse] at "+ServerGameTime;
        }
    }
}
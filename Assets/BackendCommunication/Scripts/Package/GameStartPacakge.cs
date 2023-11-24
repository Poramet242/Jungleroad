using System;
using UnityEngine;

namespace Backend
{
    public class GameStartPackage
    {
        public byte PackageType;
        public int PackageNumber;
        public double ServerGameTime;

        public Int64 GameStartUnix;

        public void FromByte(byte[] data){
            Debug.Log($"[GameStart] FromByte");
            PackageType = data[0];
            ServerGameTime = BitConverter.ToDouble(data, 1);
            PackageNumber = BitConverter.ToInt32(data, 9);
            GameStartUnix = BitConverter.ToInt64(data, 13);
        }

        public override string ToString()
        {
            return "[GameStart] at "+ServerGameTime+" unix: "+GameStartUnix;
        }
    }
}
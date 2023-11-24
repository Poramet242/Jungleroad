using System;
using UnityEngine;

namespace Backend
{
    public class TickTimeRequestPackage
    {
        public byte PackageType;

        public TickTimeRequestPackage() 
        {
            PackageType = Constant.Package_TickTimeRequest;
        }

        public byte[] ToByte()
        {
            byte[] data = new byte[1];
            data[0] = PackageType;
            return data;
        }
    }

    public class TickTimeResponsePackage
    {
        public byte PackageType;
        public int PackageNumber;
        public double ServerGameTime;

        public void FromByte(byte[] data){
            PackageType = data[0];
            ServerGameTime = BitConverter.ToDouble(data, 1);
            PackageNumber = BitConverter.ToInt32(data, 9);
        }
    }
}
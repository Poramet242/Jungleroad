using System;
using UnityEngine;

namespace Backend
{
    public class ClientReadyPackage
    {
        public byte PackageType;
        public double ClientGameTime;

        public ClientReadyPackage(double gametime)
        {
            PackageType = Constant.PacakgeType_ClientReady;
            ClientGameTime = gametime;
        }

        public byte[] ToByte()
        {
            Debug.Log($"[ClientReady] ToByte");
            byte[] data = new byte[10];

            data[0] = PackageType;
            
            int n = 1;
            byte[] gameTimeByte = BitConverter.GetBytes(ClientGameTime);
            for (int i = 0; i < gameTimeByte.Length; i++)
            {
                data[n + i] = gameTimeByte[i];
            }
            n += gameTimeByte.Length;

            return data;
        }
    }
}
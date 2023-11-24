using System;
using UnityEngine;

namespace Backend
{
    public class GameplayInputPackage
    {
        public byte PackageType;
        public double ClientGameTime;
        public int ActionSeqNumber;
        public byte Input;

        public GameplayInputPackage(byte input, double gametime, int actionNo)
        {
            PackageType = Constant.PackageType_Input;
            Input = input;
            ClientGameTime = gametime;
            ActionSeqNumber = actionNo;
        }

        public byte[] ToByte()
        {
            byte[] data = new byte[14];

            data[0] = PackageType;
            
            int n = 1;
            byte[] gameTimeByte = BitConverter.GetBytes(ClientGameTime);
            for (int i = 0; i < gameTimeByte.Length; i++)
            {
                data[n + i] = gameTimeByte[i];
            }
            n += gameTimeByte.Length;

            byte[] actionNoByte = BitConverter.GetBytes(ActionSeqNumber);
            for (int i = 0; i < actionNoByte.Length; i++)
            {
                data[n + i] = actionNoByte[i];
            }
            n += actionNoByte.Length;

            data[n] = Input;

            return data;
        }

        public override string ToString()
        {
            return "[InputPackage] ActionNo:"+ ActionSeqNumber + "Input:" + Input + " GameTime:" + ClientGameTime;
        }

        private class Debug
        {
        }
    }
}
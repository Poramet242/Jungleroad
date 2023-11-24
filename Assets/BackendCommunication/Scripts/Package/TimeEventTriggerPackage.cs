using System;
using UnityEngine;

namespace Backend
{
    public class TimeEventTriggerPackage
    {
        public byte PackageType;
        public int PackageNumber;
        public double ClientGameTime;
        public byte EventType;
        public byte Result;

        public void FromByte(byte[] data){
            Debug.Log($"[TimeEvent] FromByte");
            PackageType = data[0];
            ClientGameTime = BitConverter.ToDouble(data, 1);
            PackageNumber = BitConverter.ToInt32(data, 9);
            EventType = data[13];
            Result = data[14];
        }

        public override string ToString()
        {
            return "[TimeEventTrigger] Type: ["+EventType+"] Result: "+Result;
        }
    }
}
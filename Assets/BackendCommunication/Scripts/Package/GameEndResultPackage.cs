using System;
using UnityEngine;

namespace Backend
{
    public class GameEndResultPackage
    {
        public byte PackageType;
        public int Distance;
        public int CoinCollected;
        public double TotalReward;
        public int Stress;
        public double Health;
        public int Stamina;
        public int BoosterReward;
        public string Rank;
        public string LastRest;

        public void FromByte(byte[] data){
            PackageType = data[0];
            
            //[1:5]
            Distance = BitConverter.ToInt32(data, 1); 

            //[5:9]
            CoinCollected = BitConverter.ToInt32(data, 5); 

            //[9:17]
            TotalReward = BitConverter.ToDouble(data, 9);

            //[17:21]
            Stress = BitConverter.ToInt32(data, 17); 

            //[21:29]
            Health = BitConverter.ToDouble(data, 21);

            //[29:33]
            Stamina = BitConverter.ToInt32(data, 29); 

            //[33:37]
            BoosterReward = BitConverter.ToInt32(data, 33); 

            //[37:41] Rank
            int n = 37;
            int rankByteLeght = BitConverter.ToInt32(data, n);
            n += 4;
            Rank = System.Text.Encoding.UTF8.GetString(data, n, rankByteLeght);
            n += rankByteLeght;

            //Last Rest
            int lastRestLeght = BitConverter.ToInt32(data, n);
            n += 4;
            LastRest = System.Text.Encoding.UTF8.GetString(data, n, lastRestLeght);
            n += lastRestLeght;
        }
    }
}
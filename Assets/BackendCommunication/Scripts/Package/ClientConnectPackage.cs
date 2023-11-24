using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Backend
{
    public class ClientConnectPackage
    {
        public byte PackageType;
        public double ClientGameTime;
        public string ConnectToken;
        // public string CharacterId;
        // public string Token;

        public ClientConnectPackage(double gametime, string connectToken)
        {
            PackageType = Constant.PackageType_ClientConnect;
            ClientGameTime = gametime;
            this.ConnectToken = connectToken;
            // CharacterId = charId;
            // Token = token;
        }

        public byte[] ToByte()
        {
            Debug.Log($"[ClientConnect::ToByte] start decoding");
            //byte[] data = new byte[10];
            List<byte> dataList = new List<byte>();

            dataList.Add(PackageType);
            
            //int n = 1;
            byte[] gameTimeByte = BitConverter.GetBytes(ClientGameTime);
            dataList.AddRange(gameTimeByte);
            // for (int i = 0; i < gameTimeByte.Length; i++)
            // {
            //     data[n + i] = gameTimeByte[i];
            // }
            // n += gameTimeByte.Length;

            // ticket
            byte[] ticketBytes = Encoding.ASCII.GetBytes(ConnectToken);
            int ticketBytesLenght = ticketBytes.Length;

            byte[] ticketBytesLenghtByte = BitConverter.GetBytes(ticketBytesLenght);
            dataList.AddRange(ticketBytesLenghtByte);
            // for (int i = 0; i < ticketBytesLenghtByte.Length; i++)
            // {
            //     data[n + i] = ticketBytesLenghtByte[i];
            // }
            // n += ticketBytesLenghtByte.Length;

            // for (int i = 0; i < ticketBytes.Length; i++)
            // {
            //     data[n + i] = ticketBytes[i];
            // }
            // n += ticketBytes.Length;
            dataList.AddRange(ticketBytes);

            // // Character Id Lenght + Character Id
            // byte[] characterIdBytes = Encoding.ASCII.GetBytes(CharacterId);
            // int characterIdByteLenght = characterIdBytes.Length;

            // byte[] characterIdByteLenghtByte = BitConverter.GetBytes(characterIdByteLenght);
            // for (int i = 0; i < characterIdByteLenghtByte.Length; i++)
            // {
            //     data[n + i] = characterIdByteLenghtByte[i];
            // }
            // n += characterIdByteLenghtByte.Length;

            // for (int i = 0; i < characterIdBytes.Length; i++)
            // {
            //     data[n + i] = characterIdBytes[i];
            // }
            // n += characterIdBytes.Length;

            // // Token Lenght + Token
            // byte[] tokenBytes = Encoding.UTF8.GetBytes(Token);
            // int tokenByteLenght = tokenBytes.Length;

            // byte[] tokenByteLenghtByte = BitConverter.GetBytes(tokenByteLenght);
            // for (int i = 0; i < tokenByteLenghtByte.Length; i++)
            // {
            //     data[n + i] = tokenByteLenghtByte[i];
            // }
            // n += tokenByteLenghtByte.Length;

            // for (int i = 0; i < tokenBytes.Length; i++)
            // {
            //     data[n + i] = tokenBytes[i];
            // }
            // n += tokenBytes.Length;

            byte[] data = dataList.ToArray();
            
            Debug.Log($"[ClientConnect::ToByte] done");

            return data;
        }
    }
}
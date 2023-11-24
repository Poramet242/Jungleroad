
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;

namespace Backend
{
    public class SocketConnection : MonoBehaviour
    {
        internal Boolean socketReady = false;
        private TcpClient socket;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        public Action<string> OnMsgRecieved;
        public Action<byte[]> OnPackageRecieved;
        public Action<bool> OnConnect;

        void Update()
        {
            if (socketReady)
            {
                string receivedText = ReadSocket();
                if (receivedText != "")
                {
                    Debug.Log($"Received: {receivedText}");
                    if (OnMsgRecieved != null)
                    {
                        OnMsgRecieved(receivedText);
                    }
                    receivedText = "";
                }
            }
        }

        void OnApplicationQuit()
        {
            if (socketReady)
            {
                CloseSocket();
            }
        }

        public void SetupSocket(string hostName, int port)
        {
            try
            {
                socket = new TcpClient(hostName, port);
                stream = socket.GetStream();
                writer = new StreamWriter(stream);
                reader = new StreamReader(stream);
                socketReady = true;
                if (OnConnect != null)
                {
                    OnConnect(true);
                }
            }

            catch (Exception e)
            {
                Debug.Log("Socket error: " + e);
                if (OnMsgRecieved != null)
                {
                    OnMsgRecieved("Socket error: " + e);
                }
            }
        }

        public void SetConnectCallback(Action<bool> callback)
        {
            OnConnect = callback;
        }

        public void SetMsgReciveCallback(Action<string> callback)
        {
            OnMsgRecieved = callback;
        }

        public void SetOnPackageRecievedCallback(Action<byte[]> callback){
            OnPackageRecieved = callback;
        }

        public void WriteSocket(string msgLine)
        {
            if (!socketReady)
                return;

            String foo = msgLine + "\r\n";
            writer.Write(foo);
            writer.Flush();
        }

        public String ReadSocket()
        {
            if (!socketReady)
                return "";
            if (stream.DataAvailable)
                return reader.ReadLine();
            //return theReader.ReadToEnd();
            return "";

        }

        public void CloseSocket()
        {
            if (!socketReady)
                return;

            writer.Close();
            reader.Close();
            socket.Close();
            socketReady = false;
        }
    }
}
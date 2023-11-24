using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class WebsocketConnection : MonoBehaviour
    {
        private WebSocket webSocket;
        private string msgToSend = "msg";
        private byte[] packageToSend;
        private string hostURL = "localhost:30001";
        private bool webSocketReady = false;

        public Action<string> OnLog;
        public Action<byte[]> OnPackageRecieved;
        public Action<bool> OnConnect;
        public Action<string> OnDisconnect;

        public void InvokeConnectWebSocket(string hostURL)
        {
            Debug.Log("InvokeConnectWebSocket");
            this.hostURL = hostURL;
            ConnectWebSocket();
        }

        async void ConnectWebSocket()
        {
            const string logTag = "[WebsocketConnection::ConnectWebSocket]";
            Debug.Log($"{logTag} will connect to {hostURL}");

            webSocket = new WebSocket(hostURL);
            webSocket.OnOpen += () =>
            {
                webSocketReady = true;
                Debug.Log($"{logTag} Connection open!");
                OnConnect?.Invoke(true);
                OnLog?.Invoke("Conection opened!");
            };
            webSocket.OnError += (e) =>
            {
                Debug.LogError($"{logTag} {e}");
                OnLog?.Invoke($"Error: {e}");
                throw new Exception("error");
            };
            webSocket.OnClose += (e) =>
            {
                Debug.Log($"{logTag} Connection closed!");
                OnLog?.Invoke("Connection closed!");
                OnDisconnect?.Invoke(e.ToString());
                //UIManager.Instance.ShowError();
            };
            webSocket.OnMessage += (bytes) =>
            {
                OnLog?.Invoke("OnMessage");
                OnPackageRecieved?.Invoke(bytes);
            };

            // waiting for messages
            await webSocket.Connect();
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (webSocketReady)
            {
                webSocket.DispatchMessageQueue();
            }
#endif
        }
        
        public void SetOnLogCallback(Action<string> callback){
            OnLog = callback;
        }

        public void SetConnectCallback(Action<bool> callback)
        {
            OnConnect = callback;
        }

        public void SetDisconnectCallback(Action<string> callback)
        {
            OnDisconnect = callback;
        }

        public void SetOnPackageRecievedCallback(Action<byte[]> callback){
            OnPackageRecieved = callback;
        }

        public void SentPackageToServer(byte[] pkg)
        {
            const string logTag = "[WebSocketConnection::SentPackageToServer]";
            Debug.Log($"{logTag} Sending package type {pkg[0]} to server");
            SendWebSocketPackageAsync(pkg);
        }

        async void SendWebSocketPackageAsync(byte[] pkg) {
            const string logTag = "[WebSocketConnection::SendWebSocketPackageAsync]";
            if (webSocket.State != WebSocketState.Open) {
                Debug.LogWarning($"webSocket is not open (state:{webSocket.State})");
                return;
            }
            await webSocket.Send(pkg);
            Debug.Log($"{logTag} sent");
        }

        [Obsolete("use SendWebSocketPackageAsync instead")]
        async void SendWebSocketPackage()
        {
            if (webSocket.State == WebSocketState.Open)
            {
                // Sending bytes
                await webSocket.Send(packageToSend);
                packageToSend = null;
            }
        }

        public async void CloseConnection()
        {
            if (!webSocketReady) return;
            await webSocket.Close();
            
            OnConnect = null;
            OnLog = null;
            OnPackageRecieved = null;

            webSocketReady = false;
        }

        private void OnApplicationQuit()
        {
            CloseConnection();
        }
    }
}
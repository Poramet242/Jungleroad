using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using System.Text;

namespace Backend
{
    public class BackendComm : MonoBehaviour
    {
        private enum ConnectionType {
            WebSocket,
            Socket
        }

        [SerializeField]
        private ConnectionType connectionType;
        [SerializeField]
        private WebsocketConnection websocket;
        [SerializeField]
        private SocketConnection socket;

        private string backendAddress;
        private bool commReady = false;

        private Action<bool> onConnectedRelay;
        private Action<string> onDisconnectedRelay;
        private Action<string> onLogUpdate;
        private Action<byte[]> onPkgRecieved;

        public static BackendComm instance;

        private int currentActionSeqNumber;

        private void Awake() {
            if(instance != null){
                Destroy(gameObject);
                return;
            }    
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetBackendAddress(string addr){
            backendAddress = addr;
            //NOTE: chanyutl - just remove port variable, will be fixed later....
        }

        public void ConnectToBackend(){
            currentActionSeqNumber = 0;
            switch(connectionType){
                case ConnectionType.WebSocket:
                    websocket.SetConnectCallback(onConnected);
                    websocket.SetDisconnectCallback(onDisconnected);
                    websocket.SetOnPackageRecievedCallback(OnPackageRecived);
                    websocket.SetOnLogCallback(onLogUpdate);
                    websocket.InvokeConnectWebSocket(backendAddress);
                break;
                case ConnectionType.Socket:
                    socket.SetupSocket(backendAddress, 0);
                    socket.SetConnectCallback(onConnected);
                    socket.SetOnPackageRecievedCallback(OnPackageRecived);
                break;
            }
        }

        // Call when web socket is successfully connect to backend
        public void SetOnConnectedRelay(Action<bool> callback){
            onConnectedRelay = callback;
        }

        public void SetOnDisconnectedRelay(Action<string> callback){
            onDisconnectedRelay = callback;
        }

        // Call when Backend Comm write log, for debug
        public void SetOnLogUpdate(Action<string> callback){
            onLogUpdate = callback;
        }

        // Call when Recived Package (byte[]) from backend
        public void SetOnPackageRecieved(Action<byte[]> callback){
            onPkgRecieved = callback;
        }

        private void onConnected(bool connect){
            Debug.Log($"Connect to backend via {connectionType}");
            commReady = connect;
            onConnectedRelay?.Invoke(true);
            onLogUpdate?.Invoke("Connected");
        }

        private void onDisconnected(string closeCode){
            commReady = false;
            onDisconnectedRelay?.Invoke(closeCode);
            onLogUpdate?.Invoke("Connection Closed");
        }

        private void OnPackageRecived(byte[] pkg) {
            onPkgRecieved?.Invoke(pkg);
        }

        public void SentPacakge(byte[] pkg){
            if(!commReady) return;

            switch(connectionType){
                case ConnectionType.WebSocket:
                    if(websocket == null) return;
                    websocket.SentPackageToServer(pkg);
                break;
                case ConnectionType.Socket:
                break;
            }
        }

        public void SendClientConnect(double gameTime, string ConnectToken){
            ClientConnectPackage pkg = new ClientConnectPackage(gameTime, ConnectToken);
            SentPacakge(pkg.ToByte());
        }

        public void SendClientReady(double gameTime){
            ClientReadyPackage pkg = new ClientReadyPackage(gameTime);
            SentPacakge(pkg.ToByte());
        }

        // SendInput returns seqNumber of the input packet
        public int SendInput(byte input, double gameTime){
            currentActionSeqNumber++;

            GameplayInputPackage pkg = new GameplayInputPackage(input, gameTime, currentActionSeqNumber);
            SentPacakge(pkg.ToByte());

            return currentActionSeqNumber;
        }

        public void SendOffScreenReport(double gameTime){
            Debug.Log("SendOffScreenReport");
            OffScreenReportPackage pkg = new OffScreenReportPackage(gameTime);
            SentPacakge(pkg.ToByte());
        }

        public void ResetActionSeqNumber() {
            currentActionSeqNumber = 0;
        }

        public void SendConfirmGameEndResult() {
            ConfirmGameEndResultPackage pkg = new ConfirmGameEndResultPackage();
            SentPacakge(pkg.ToByte());
        }

        public void SendMessagePackage(string msg){
            List<byte> package = new List<byte>{};
            package.Add(Constant.PackageType_Message);
            byte[] msgByte = Encoding.ASCII.GetBytes(msg);
            package.AddRange(msgByte);

            SentPacakge(package.ToArray());
        }

        public void SendServerTickTimeRequest(){
            var pkg = new TickTimeRequestPackage();
            SentPacakge(pkg.ToByte());
        }

        public void CloseConnection() {
            websocket.CloseConnection();
        }
    }
}
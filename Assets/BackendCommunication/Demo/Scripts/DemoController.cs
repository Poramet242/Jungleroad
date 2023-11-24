using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Backend;
using System;
using System.Text;

public class DemoController : MonoBehaviour
{
    public TMP_InputField IpField;
    public TMP_InputField PortField;
    public Button ConnectBtn;

    public TMP_InputField LogField;
    public TMP_InputField InfoField;
    public ScrollRect InfoFieldScrollRect;

    public Button UpBtn;
    public Button LeftBtn;
    public Button RightBtn;
    public Button BackBtn;

    public TMP_InputField MsgField;
    public Button SendMsgBtn;
    public MapGenerator MapGenerator;
    public MapController MapController;
    public PlayerController PlayerController;

    private string logs;

    private DateTime gameStartTime;

    private bool connected;
    private bool gameActive;
    private byte inputDirection;
    private bool waitingForBackend = false;
    private Coroutine waitForBackendConfirmRoutine;

    void Start()
    {
        ConnectBtn.onClick.AddListener(() =>
        {
            OnConnectClick();
        });

        UpBtn.onClick.AddListener(() =>
        {
            SentInputAndWait(Constant.Input_Forward);
        });
        LeftBtn.onClick.AddListener(() =>
        {
            SentInputAndWait(Constant.Input_Left);
        });
        RightBtn.onClick.AddListener(() =>
        {
            SentInputAndWait(Constant.Input_Right);
        });
        BackBtn.onClick.AddListener(() =>
        {
            SentInputAndWait(Constant.Input_Backward);
        });

        SendMsgBtn.onClick.AddListener(() =>
        {
            sendMessagePackage(MsgField.text);
        });
    }

    //On connect button click
    private void OnConnectClick()
    {
        int targetPort = int.Parse(PortField.text);

        // set ipaddress กับ port ที่ใช้ต่อ backend
        Backend.BackendComm.instance.SetBackendAddress(IpField.text);

        // set callback สำหรับตอนที่ต่อกับ backend comm ได้
        Backend.BackendComm.instance.SetOnConnectedRelay(this.OnConnected);
        // onlogupdated ใช้สำหรับ debug ใช้ให้ backend comm พ่น log กลับมาที่ game manager, optional ใส่ก็ได้ไม่ใส่ก็ได้
        Backend.BackendComm.instance.SetOnLogUpdate(this.OnLogUpdated);
        // on package recieved เป็น callback ที่ใช้รับ package ที่ตอบกลับมาจาก gameserver
        Backend.BackendComm.instance.SetOnPackageRecieved(this.OnPkgRecived);

        // สั่งเชื่อมต่อไป backend
        Backend.BackendComm.instance.ConnectToBackend();
    }

    // ถ้าต่อ websocket ได้แล้ว backend comm จะเรียก callback ตัวนี้(ตามที่ใส่ไว้ด้านบน)
    private void OnConnected(bool success)
    {
        if (success)
        {
            gameStartTime = DateTime.Now;
            connected = true;

            // ส่ง package client connect ไปบอก gameserver ว่าเชื่อมต่อแล้ว พร้อมจะเริ่มสร้าง map
            sentClientConnectToServer();
        }
    }

    private void OnLogUpdated(string log)
    {
        setLogText("> [log] " + log);
    }

    private void OnMessageRecived(string msg)
    {
        setLogText("> [msg] " + msg);
    }

    private void OnPkgRecived(byte[] pkg)
    {
        switch (pkg[0])
        {
            case Backend.Constant.PackageType_ConnectResponse: // package จาก gameserver ตอบกลับจาก package client connect ในนี้จะมี seed ส่งกลับมาด้วย
                onRecievedConnectResponsePackage(pkg);
                break;
            case Backend.Constant.PackageType_GameStart: // gameserver บอกให้ client เริ่มเกมได้เลย
                onRecievedGameStartPackage(pkg);
                break;
            case Backend.Constant.PackageType_Message: // gameserver ส่ง msg กลับมา client (ของเก่าไม่น่าใช้แล้วในเกม)
                onRecievedMessagePackage(pkg);
                break;
            case Backend.Constant.PackageType_ActionResponse: // response ของ package game input จาก gameserver บอกผลของการเคลื่อนที่
                onRecievedActionResponsePackage(pkg);
                break;

                // ยังขาดอีก 1 package ที่จะยิงมาจาก gameserver คือตัว time event ที่นับอยู่บน server โดนชน หรือหลุดจอมั้ย
        }
    }

    private void onRecievedConnectResponsePackage(byte[] pkg)
    {
        Debug.Log($"Recieved Connect Response");
        ConnectResponsePackage connectResponse = new ConnectResponsePackage { };
        connectResponse.FromByte(pkg);

        // เอา seed ที่ตอบกลับมาใส่ map generator
        MapGenerator.SetSeed(connectResponse.Seed);
        // สร้าง 50 lane แรก, ตอนนี้ fix อยู่, อาจจะปรับให้ gameserver ส่งมาพร้อมกับ seed บอกว่าสร้างเท่าไหร่ หรือไม่ก็นัดกับ client เลยว่าสร้างชุดแรกเอากี่ lane
        MapGenerator.GenerateLevel(50);
        // การสร้าง lane เพิ่มตอนเล่นไปใกล้หมดที่สร้างไว้แล้ว อาจจะต้องคุยกันว่าจะทำแบบนัดกันสร้างเองตาม algo ทั้งสองฝั่ง หรือให้ server ส่งเป็น package มาบอก

        // เอา map ทั้งหมดที่ gen ไว้ใน map gen ไปให้ตัวสร้างด่านวางใน scene
        //MapController.SetMap(MapGenerator.GetAllLevel());

        // วางครบแล้วก็ส่งไปบอก gameserver ว่าพร้อมเล่นแล้ว รอตอบกลับ gamestart
        sentClientReadyToServer();
    }

    private void onRecievedGameStartPackage(byte[] pkg)
    {
        Debug.Log($"Recieved Game Start");
        setLogText("Game Start!");

        // เริ่มเกม
        MapController.StartGame();
        gameActive = true;
    }

    private void onRecievedMessagePackage(byte[] pkg)
    {
        Debug.Log($"Recieved Message");
        byte[] msgByte = new byte[pkg.Length - 1];
        for (int i = 1; i < pkg.Length; i++)
        {
            msgByte[i - 1] = pkg[i];
        }
        string message = Encoding.ASCII.GetString(msgByte);

        setLogText(message);
    }

    private void onRecievedActionResponsePackage(byte[] pkg)
    {
        Debug.Log($"Recieved Action Response");

        ActionResponsePackage package = new ActionResponsePackage { };
        package.FromByte(pkg);

        setLogText(package.ToString());

        if (waitingForBackend)
        {
            if(package.CanMove){
                switch (inputDirection)
                {
                    case Constant.Input_Forward:
                        PlayerController.MoveForward();
                        break;
                    case Constant.Input_Backward:
                        PlayerController.MoveBack();
                        break;
                    case Constant.Input_Left:
                        PlayerController.MoveLeft();
                        break;
                    case Constant.Input_Right:
                        PlayerController.MoveRight();
                        break;
                }
            }

            if(package.MoveResult != Constant.MoveResult_Deny && package.MoveResult != Constant.MoveResult_OK){
                MapController.StopGame();
                gameActive = false;
            }

            waitingForBackend = false;
        }
    }

    private void sentClientConnectToServer()
    {
        if (!connected) return;
        double diffInSeconds = (DateTime.Now - gameStartTime).TotalSeconds;
        Backend.BackendComm.instance.SendClientConnect(diffInSeconds, "");
    }

    private void sentClientReadyToServer()
    {
        const string logTag = "[DemoController::sentClientReadyToServer]";
        if (!connected) return;
        double diffInSeconds = (DateTime.Now - gameStartTime).TotalSeconds;
        Backend.BackendComm.instance.SendClientReady(diffInSeconds);
        Debug.Log($"{logTag} sent clientReady packet");

        //TODO: chanyutl - confirm that setting gameStartTime here is correct?
        // as we see in server, the GameLogic.StartedAt is set whenever it receive clientReady packet
        DataManager.Instance.gameStartTime = DateTime.Now;
    }

    private void sendInput(byte input)
    {
        if (!connected) return;
        double diffInSeconds = (DateTime.Now - gameStartTime).TotalSeconds;
        Backend.BackendComm.instance.SendInput(input, diffInSeconds);
    }

    private void sendMessagePackage(string msg)
    {
        Backend.BackendComm.instance.SendMessagePackage(msg);
    }

    private void setLogText(string msg)
    {
        logs = logs + msg + "\n";
        InfoField.SetTextWithoutNotify(logs);
    }

    private void SentInputAndWait(byte input)
    {
        if (!gameActive) return;
        if (waitingForBackend) return;

        sendInput(input);
        inputDirection = input;
        waitingForBackend = true;
    }


}

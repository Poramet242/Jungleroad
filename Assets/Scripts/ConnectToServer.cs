using Backend;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectToServer : MonoBehaviour
{
    public enum HostConfigSettings
    {
        Localhost,
        LocalHostSingleHub,
        LocalHostAgones,
        RemoteDev,
        RemoteDevAgones,
        RemoteStagingAgones,
        RemoteProductionAgones, // <- NOTE: chanyutl - use this config for production build
        Custom,
    }

    public enum GameState{
        Disconnected = 0,
        Allocating = 1,
        Connecting = 2,
        Connected = 3,
        Playing = 4,
        Dead = 50,
        WaitForGameEndResult = 51,
        GotGameEndResult = 52
    }

    public struct WebSocketProxySetting {
        public bool Enable;
        public bool secure;
        public string URL; // url of proxy server 
    }

    [Serializable]
    struct HostConfig
    {
        /// IsSingleHubHub specify that host already has ready-to-connect Hub, so gameServerHostName must be include both host(URL or IP) and port
        public bool IsSingleHub;
        /// useAllocator specify that it will request for gameserver (Hub) allocation from allocator service instead of hubController
        /// this requires allocatorHostURL to be valid
        /// otherwise if useAllocator set as false, it will request for new Hub creation from hubcontroller service
        public bool useAllocator;
        /// host URL example http://<ipAddress|dnsname>:<port>"
        public string hubControllerHostURL;
        /// hostname is just only a dns name or ip address without prefix http:// or https://"
        public string gameServerHostName;
        /// Allocator service host URL example http://<ipAddress|dnsname>:<port>"
        public string allocatorHostURL;
        /// Allocator request fleet name defines the fleetname send as request to Allocator
        public string allocatorRequestFleetName;
    }

    [SerializeField]
    HostConfig cfgLocalhost = new HostConfig
    {
        IsSingleHub = false,
        useAllocator = false,
        hubControllerHostURL = "http://localhost:9099",
        gameServerHostName = "localhost",
        allocatorHostURL = "http://localhost:9001",
        allocatorRequestFleetName = "-",
    };
    [SerializeField]
    HostConfig cfgLocalhostSingleHub = new HostConfig
    {
        IsSingleHub = true,
        useAllocator = false,
        hubControllerHostURL = "-",
        gameServerHostName = "localhost:30101",
        allocatorRequestFleetName = "-",
    };
    [SerializeField]
    HostConfig cfgLocalhostAgones = new HostConfig
    {
        IsSingleHub = false,
        useAllocator = true,
        hubControllerHostURL = "-",
        gameServerHostName = "-",
        allocatorHostURL = "localhost:9001",
        allocatorRequestFleetName = "simple-game-server",
    };
    [SerializeField]
    HostConfig cfgRemoteDev = new HostConfig
    {
        IsSingleHub = false,
        useAllocator = false,
        hubControllerHostURL = "http://167.99.76.142:9099",
        gameServerHostName = "167.99.76.142",
        allocatorHostURL = "-",
        allocatorRequestFleetName = "-",
    };
    [SerializeField]
    HostConfig cfgRemoteDevAgones = new HostConfig
    {
        IsSingleHub = false,
        useAllocator = true,
        hubControllerHostURL = "-",
        gameServerHostName = "-",
        allocatorHostURL = "http://167.99.76.142:9001",
        allocatorRequestFleetName = "simple-game-server",
    };
    [SerializeField]
    HostConfig cfgRemoteStagingAgones = new HostConfig
    {
        IsSingleHub = false,
        useAllocator = true,
        hubControllerHostURL = "-",
        gameServerHostName = "-",
        allocatorHostURL = "https://stagingxyz-allocator.jungleroad.io",
        allocatorRequestFleetName = "simple-game-server-staging",
    };
    [SerializeField]
    HostConfig cfgRemoteProdutionAgones = new HostConfig
    {
        IsSingleHub = false,
        useAllocator = true,
        hubControllerHostURL = "-",
        gameServerHostName = "-",
        allocatorHostURL = "https://allocator.jungleroad.io",
        allocatorRequestFleetName = "simple-game-server-prod",
    };

    public HostConfigSettings hostConfigSetting;
    private HostConfig activeHostCfg;

    [Header("Custom host config")]
    [Tooltip("host URL example http://<ipAddress|dnsname>:<port>")]
    [SerializeField] string hubControllerHostURL;
    [Tooltip("hostname is just only a dns name or ip address without prefix http:// or https://")]
    [SerializeField] string gameServerHostName;
    private string logs;

    private static ConnectToServer s_instance;

    // onReceiveActionResponse will broadcast an action response package
    private Queue<ActionResponsePackage> actionResponseQueue;
    public event Action<ActionResponsePackage> onReceiveActionResponse;

    public event Action<byte[]> onPkgReceivedHandler;

    private bool waitForGameServerDisconnect = false;
    private Coroutine actionResponseQueueBroadcastRoutineObj;
    
    private string connectToken;

    private GameState gameState = GameState.Disconnected;

    public static ConnectToServer getInstance()
    {
        return s_instance;
    }

    void Awake()
    {
        if (s_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        s_instance = this;
        actionResponseQueue = new Queue<ActionResponsePackage>();

        activeHostCfg = hostConfigSetting switch
        {
            HostConfigSettings.Localhost => cfgLocalhost,
            HostConfigSettings.LocalHostSingleHub => cfgLocalhostSingleHub,
            HostConfigSettings.LocalHostAgones => cfgLocalhostAgones,
            HostConfigSettings.RemoteDev => cfgRemoteDev,
            HostConfigSettings.RemoteDevAgones => cfgRemoteDevAgones,
            HostConfigSettings.RemoteStagingAgones => cfgRemoteStagingAgones,
            HostConfigSettings.RemoteProductionAgones => cfgRemoteProdutionAgones,
            HostConfigSettings.Custom => new HostConfig
            {
                gameServerHostName = gameServerHostName,
                hubControllerHostURL = hubControllerHostURL,
            },
            _ => throw new System.ArgumentException($"invalid hostConfigSettings: {hostConfigSetting}"),
        };

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // actionResponseQueueBroadcasterRoutine is taken into account to give another management layer of control
        // for example, when we would drop some action response, if we found the current action can be replaced with the old one 
        actionResponseQueueBroadcastRoutineObj = StartCoroutine(actionResponseQueueBroadcasterRoutine());
    }

    // InvokeConnectFromUI is for link to call connect function from inspector
    public void StartConnectProcess(string accessToken, string characterID, Action<bool, string> callback)
    {
        SetGameState(GameState.Allocating);

        if (activeHostCfg.IsSingleHub)
        {
            ConnectToHub(activeHostCfg.gameServerHostName, "", (success) => callback?.Invoke(success, ""));
            return;
        }

        if (activeHostCfg.useAllocator) 
        {
            StartCoroutine(AllocateGameServerAndConnect(activeHostCfg.allocatorHostURL, accessToken, characterID, callback));
            return;
        }
        
        StartCoroutine(CreateNewHubAndConnect(activeHostCfg.gameServerHostName, callback));
    }

    public void SetGameState(GameState state){
        Debug.Log($"[ConnectToServer] SetGameState From {gameState} to {state}");
        gameState = state;
    }

    public GameState GetGameState(){
        return gameState;
    }

    public bool IsGameEnding(){
        return (int)(object)gameState >= (int)(object)GameState.Dead;
    }

    // Connect connects to server
    private IEnumerator AllocateGameServerAndConnect(string host, string accessToken, string characterID, Action<bool, string> callback)
    {
        waitForGameServerDisconnect = false;

        const string logTag = "[ConnectToServer::AllocateGameServerAndConnect]";

        Debug.Log($"requesting game server allocation... url: {host} accessToken: {accessToken} characterID: {characterID}");
        var resp = new AllocateGameServerResponse();
        yield return AllocateGameServer(
            accessToken: accessToken == null ? "": accessToken, 
            characterID: characterID == null ? "": characterID, 
            cb: (resp_) => resp = resp_
        );
        
        if (!resp.success)
        {
            if (resp.error.isInsufficientGameServerError()) 
            {
                Debug.LogWarning("there is no available room now, please wait for next xxx minutes");
                //TODO: Ryu/Gun - please inform player that there is no available room now, please wait for next xxx minutes
                CharacterManager.instance.ErrorFullGameRoom(10);
                callback?.Invoke(false, "Game server does not available");
            }
            else
            {
                Debug.LogError($"allocate game room... failed with error: {resp.error.eMsg} ({resp.error.eID})");
                CharacterManager.instance.mmum.ShowErrorPopUp("Request to allocate game server failed");
                callback?.Invoke(false, "Request to allocate game server failed");
            }
            yield break;
        }

        var gsAddress = resp.address;
        var gsPort = resp.port;
        var gsConnectToken = resp.connectToken;
        Debug.Log($"{logTag} allocated game server... done (access address: {gsAddress} port: {gsPort})");

        var url = $"ws:{gsAddress}:{gsPort}";
        if (!string.IsNullOrEmpty(resp.fullWSIngressURL)) 
        {
            url = resp.fullWSIngressURL;
        }

        ConnectToHub(url, gsConnectToken, (success) => {
            callback.Invoke(success, !success ? "failed to connect to game server" : "");
        });
    }

    // Connect connects to server
    private IEnumerator CreateNewHubAndConnect(string host, Action<bool, string> callback)
    {
        waitForGameServerDisconnect = false;

        const string logTag = "[ConnectToServer::CreateNewHubAndConnect]";

        Debug.Log($"creating new hub/gameRoom...");
        CreateNewHubResponse createHubResp = new CreateNewHubResponse();
        yield return CreateNewHub((resp) => createHubResp = resp);
        if (!createHubResp.success)
        {
            throw new Exception("create new hub/gameRoom... failed");
        }

        var port = createHubResp.port;
        Debug.Log($"{logTag} created new hub/gameRoom... done (access port: {port})");

        ConnectToHub($"{host}:{port}", "", (success) => {
            callback?.Invoke(success, !success ? "failed to connect to game server" : "");
        });
    }

    private Action<bool> connectToBackendCallback;
    /// ConnectToHub ...
    /// {param: connectToken} connectToken is required for identify that this use is the same as the one who requested allocation
    private void ConnectToHub(string url, string connectToken, Action<bool> connectCallback) 
    {
        SetGameState(GameState.Connecting);

        this.connectToken = connectToken;
        this.connectToBackendCallback = connectCallback;

        Backend.BackendComm.instance.SetBackendAddress(url);

        Backend.BackendComm.instance.SetOnConnectedRelay(this.OnConnected);
        Backend.BackendComm.instance.SetOnDisconnectedRelay(this.OnDisconnected);
        Backend.BackendComm.instance.SetOnLogUpdate(this.OnLogUpdated);
        Backend.BackendComm.instance.SetOnPackageRecieved(this.OnPkgRecived); //*
        Backend.BackendComm.instance.ConnectToBackend();
    }

    // Disconnect calls to disconnect from server
    // please note that 
    private void Disconnect()
    {
        Backend.BackendComm.instance.CloseConnection();
    }

    private void OnConnected(bool success)
    {
        if (success)
        {
            SetGameState(GameState.Connected);

            Debug.Log("Connected! sent client connect to sever");
            Backend.BackendComm.instance.SendClientConnect(
                gameTime: 0, 
                ConnectToken: connectToken
            );
        }
        else 
        {
            connectToBackendCallback?.Invoke(false);
        }
    }

    private void OnDisconnected(string closeCode)
    {
        if (waitForGameServerDisconnect)
        {
            Debug.Log("Disconnected! Ready to restart for new game");
        }
        else{
            Debug.Log($"Disconnected from connection CloseCode: {closeCode}");
            if(UIManager.Instance != null){
                UIManager.Instance.ShowErrorMessageObject(closeCode);
            }
        }
    }

    private void OnLogUpdated(string log)
    {
        SetLogText("> [log] " + log);
    }

    private void OnMessageRecived(string msg)
    {
        SetLogText("> [msg] " + msg);
    }
    private void SetLogText(string msg)
    {
        logs = logs + msg + "\n";
        //LogField.SetTextWithoutNotify(logs); // LogField = TextField & use to notification Log
    }

    private void OnPkgRecived(byte[] pkg)
    {
        onPkgReceivedHandler?.Invoke(pkg);

        const string logTag = "[OnPkgRecived]";
        Debug.Log($"{logTag} Package Recived");
        if (pkg == null || pkg.Length == 0)
        {
            Debug.Log($"{logTag} got empty packet... done");
            return;
        }

        Debug.Log($"pkg[0]: {pkg[0]} Length: {pkg.Length}");

        switch (pkg[0])
        {
            case Backend.Constant.PackageType_ConnectResponse: // package จาก gameserver ตอบกลับจาก package client connect ในนี้จะมี seed ส่งกลับมาด้วย
                Debug.Log($"{logTag} Recieved Connect Response");
                ConnectResponsePackage connectResponse = new ConnectResponsePackage { };
                connectResponse.FromByte(pkg);

                Debug.Log($"{logTag} seed = {connectResponse.Seed}");

                DataManager.Instance.nextSeed = new SetAndGetOnce<uint>(connectResponse.Seed);
                connectToBackendCallback?.Invoke(true);
                break;
            case Backend.Constant.PackageType_GameStart: // gameserver บอกให้ client เริ่มเกมได้เลย
                SetGameState(GameState.Playing);
                // เริ่มเกม
                Debug.Log($"Recieved Game Start");
                SetLogText("Game Start!");
                DataManager.Instance.gameStartTime = DateTime.Now;
                break;
            case Backend.Constant.PackageType_Message:
                Debug.Log($"Recieved Message");
                byte[] msgByte = new byte[pkg.Length - 1];
                Array.Copy(pkg, 1, msgByte, 0, pkg.Length - 1);
                string message = Encoding.ASCII.GetString(msgByte);
                SetLogText(message);
                break;

            case Backend.Constant.PackageType_ActionResponse:
                Debug.Log($"Recieved Action Response");
                ActionResponsePackage package = new ActionResponsePackage { };
                package.FromByte(pkg);
                Debug.Log($"<COLOR=Yellow> {logTag} {package.HumanReadable()} </COLOR>");
                SetLogText(package.HumanReadable());
                Debug.Log($"<COLOR=Yellow> {logTag} {package.MoveResult} </COLOR>");
                actionResponseQueue.Enqueue(package);
                break;
            case Backend.Constant.PackageType_TimeEventTrigger:
                Debug.Log($"<COLOR=Red> Recieved Time Event Trigger </COLOR>");
                TimeEventTriggerPackage timeEventPkg = new TimeEventTriggerPackage();
                timeEventPkg.FromByte(pkg);

                //hit = car  drow = log
                if (timeEventPkg.Result == Constant.MoveResult_Hit)
                {
                    Debug.LogError($"Dead in server [TimeEvent: MoveResult_Hit]  [gameTime: {GameWorld.getGameTime()}]");
                    SetGameState(GameState.Dead);
                    PlayerMenager.instance.ShowAniamtionDead(1);
                }
                else if (timeEventPkg.Result == Constant.MoveResult_HIT_GATOR)
                {
                    Debug.LogError($"Dead in server [TimeEvent: MoveResult_HIT_GATOR]  [gameTime: {GameWorld.getGameTime()}]");
                    SetGameState(GameState.Dead);
                    PlayerMenager.instance.ShowAniamtionDead(1);
                }
                else if (timeEventPkg.Result == Constant.MoveResult_HIT_TRAIN)
                {
                    Debug.LogError($"Dead in server [TimeEvent: MoveResult_HIT_TRAIN]  [gameTime: {GameWorld.getGameTime()}]");
                    SetGameState(GameState.Dead);
                    PlayerMenager.instance.ShowAniamtionDead(1);
                }
                else if (timeEventPkg.Result == Constant.MoveResult_Drown)
                {
                    Debug.LogError($"Dead in server [TimeEvent: MoveResult_Drown] [gameTime: {GameWorld.getGameTime()}]");
                    SetGameState(GameState.Dead);
                    PlayerMenager.instance.ShowAniamtionDead(2);
                }
                else if (timeEventPkg.Result == Constant.MoveResult_OffscreenSide)
                {
                    Debug.LogError($"Dead in server [TimeEvent: MoveResult_OffscreenSide] [gameTime: {GameWorld.getGameTime()}]");
                    SetGameState(GameState.Dead);
                    PlayerMenager.instance.ShowAniamtionDead(2);
                }
                else if (timeEventPkg.Result == Constant.MoveResult_OffscreenBack)
                {
                    Debug.LogError($"Dead in server [TimeEvent: MoveResult_OffscreenBack] [gameTime: {GameWorld.getGameTime()}]");
                    SetGameState(GameState.Dead);
                    PlayerMenager.instance.ShowAniamtionDead(3);
                }
                break;
            case Backend.Constant.Package_GameEndResult:
                Debug.Log($"Recieved GameEnd Result");

                SetGameState(GameState.GotGameEndResult);

                GameEndResultPackage result = new GameEndResultPackage();
                result.FromByte(pkg);
                //TODO use result in GameEndResultPackage to show
                UIManager.Instance.ShowCalculateResult(result);
                Backend.BackendComm.instance.SendConfirmGameEndResult();
                //TODO show result here
                waitForGameServerDisconnect = true;
                //TODO clear pkg number here
                BackendComm.instance.ResetActionSeqNumber();
                break;
        }
    }

    private IEnumerator actionResponseQueueBroadcasterRoutine()
    {
        while (true)
        {
            if (actionResponseQueue.Count == 0)
            {
                yield return null;
                continue;
            }

            if (onReceiveActionResponse == null)
            {
                yield return null;
                continue;
            };

            while (actionResponseQueue.Count > 0)
            {
                var ar = actionResponseQueue.Dequeue();
                if (ar == null) continue;
                onReceiveActionResponse(ar);
            }

            yield return null;
        }
    }

    [System.Serializable]
    public struct CreateNewHubResponse
    {
        public int port;
        public bool success;
        public string errMsg;
    }

    private IEnumerator CreateNewHub(System.Action<CreateNewHubResponse> cb)
    {
        const string logTag = "[CreateNewHub]";
        WWWForm dummy = new WWWForm();
        dummy.AddField("dummy", "dummy");

        using (UnityWebRequest www = UnityWebRequest.Post($"{activeHostCfg.hubControllerHostURL}/api/v1/hub/create", dummy))
        {
            Debug.Log($"{logTag} sending create hub request...");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"{logTag} got error: {www.error}");
                cb?.Invoke(new CreateNewHubResponse()
                {
                    success = false,
                    errMsg = www.error,
                });
                yield break;
            }

            Debug.Log($"{logTag} sending create hub request... success with response...\n{www.downloadHandler.text}");
            var resp = JsonUtility.FromJson<CreateNewHubResponse>(www.downloadHandler.text);
            Debug.Log($"{logTag} decode response -> port: {resp.port}");
            resp.success = true;
            resp.errMsg = string.Empty;
            cb?.Invoke(resp);
        }
    }

    
    public class AllocationError {
        public const int ErrID_AllocateFailed = 101;
		public const int ErrID_InsufficientGameServer = 102;
		public const int ErrID_InvalidFleetName = 103;
		public const int ErrID_NotWhitelistedFleetName = 104;

        public int eID;
        public string eMsg;

        public static AllocationError unknown() {
            return new AllocationError(){
                eID = 0,
                eMsg = "unknow error",
            };
        }

        public static AllocationError fromJSON(SimpleJSON.JSONObject v) {
            return new AllocationError(){
                eID = v["eID"].AsInt,
                eMsg = v["eMsg"].Value,
            };
        }

        public bool isInsufficientGameServerError() {
            return eID == ErrID_InsufficientGameServer;
        }
    }
    public struct AllocateGameServerResponse {
        public bool success;
        public AllocationError error;

        public string address;
        public int port;
        public string connectToken;
        public string gameServerName;
        public string ingressHostURL;
        public string fullWSIngressURL;

        public static AllocateGameServerResponse withError(SimpleJSON.JSONObject errJSON) {
            return new AllocateGameServerResponse{
                success = false,
                error = errJSON != null ?
                    AllocationError.fromJSON(errJSON) :
                    AllocationError.unknown(),
            };
        }

        public static AllocateGameServerResponse withSuccess(SimpleJSON.JSONObject body) {
            var allocation = body["allocation"].AsObject;
            return new AllocateGameServerResponse{
                success = true,
                address = allocation["ip"].Value,
                port = allocation["port"].AsInt,
                gameServerName = allocation["gameServerName"].Value,
                ingressHostURL = allocation["ingressHostURL"].Value,
                fullWSIngressURL = allocation["fullWSIngressURL"].Value,
                connectToken = body["connectToken"].Value,
            };
        }
    }

    public IEnumerator AllocateGameServer(string accessToken, string characterID, System.Action<AllocateGameServerResponse> cb)
    {
        const string logTag = "[AllocateGameServer]";

        var jObj = new SimpleJSON.JSONObject();
        jObj["accessToken"] = accessToken;
        jObj["characterID"] = characterID;
        var jsonBody = jObj.ToString();
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        var req = new UnityWebRequest(
            url: $"{activeHostCfg.allocatorHostURL}/api/v1/allocate?fleetName={activeHostCfg.allocatorRequestFleetName}"
        );
        req.method = "POST";
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        yield return req.SendWebRequest();
        while (req.result == UnityWebRequest.Result.InProgress) yield return null;
        if (req.result != UnityWebRequest.Result.Success)
        {
            var errJSON = SimpleJSON.JSON.Parse(req.downloadHandler.text).AsObject;
            Debug.Log($"{logTag} got error: {req.error} body: {errJSON.ToString()}");
            CharacterManager.instance.mmum.ShowErrorPopUp("Connect failed: Please try again");
            cb?.Invoke(AllocateGameServerResponse.withError(errJSON));
            req.Dispose();  
            yield break;
        }

        Debug.Log($"{logTag} sending allocate gameserver request... success with response...\n{req.downloadHandler.text}");
        cb?.Invoke(AllocateGameServerResponse.withSuccess(SimpleJSON.JSON.Parse(req.downloadHandler.text).AsObject));
        req.Dispose();
    }
}

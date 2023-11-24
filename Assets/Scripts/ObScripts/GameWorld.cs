using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGameWorldUpdatable {
    void GameUpdate(float gameTime);
}

public class GameWorld: MonoBehaviour {
    [SerializeField]
    long gameStart; // unix time stamp
    
    [SerializeField]
    public float gameTime { get; private set; }  // delta time = now - gameStart
    
    [SerializeField]
    bool ready = false;

#if UNITY_EDITOR
    [SerializeField] bool enableGameTimeSimulation = false;
    [SerializeField] float simulatedGameTime = .0f;
#endif
    public static float getGameTime() => activeWorld == null ? 0: activeWorld.gameTime;
    private static GameWorld activeWorld;
    private float lastReceviedGameTime;

    private DateTime previousTickTimeRequestAt;
    private double previousRtt;
    private double sumRtt;
    private double avgRtt;
    private double tickTimeCount = 0;

    void Awake() {
        activeWorld = this;
        ConnectToServer.getInstance().onPkgReceivedHandler += OnPkgReceived; 
    }

    public void setReady() {
        ready = true;
    }

    public void setGameTime(float gt) {
        gameTime = gt;
    }

    void Start() {
        StartCoroutine(RequestServerTick());
    }

    public void Update() {
        if (!ready) return;

        gameTime += Time.deltaTime;
        
#if UNITY_EDITOR
        if (enableGameTimeSimulation) {
            gameTime = simulatedGameTime;
        }
#endif

        if(toRemoveMovableObjects.Count > 0){
            for(int i = moveableObjects.Count - 1; i >= 0; i--) {
                for(int j = 0; j < toRemoveMovableObjects.Count; j++){
                    if(System.Object.ReferenceEquals(moveableObjects[i], toRemoveMovableObjects[j])) {
                        moveableObjects.RemoveAt(i);
                        break;
                    }
                }
            }

            toRemoveMovableObjects.Clear();
        }

        for (int i=0; i<moveableObjects.Count; i++) {
            moveableObjects[i].Update(gameTime);
        }
    }

    private System.Collections.IEnumerator RequestServerTick() {
        while (true) {
            yield return new WaitForSeconds(0.25f);
            if (!ready) continue;
            var p = new Backend.TickTimeRequestPackage();
            Backend.BackendComm.instance.SendServerTickTimeRequest();
            previousTickTimeRequestAt = DateTime.Now;
        }
    }

    private void OnPkgReceived(byte[] pkg)
    {
        if (this == null) return;
        if (!this.enabled) return;
        const string logTag = "[GameWorld]";
        switch (pkg[0])
        {
            case Backend.Constant.PackageType_GameStart:
                {
                    Debug.Log($"{logTag} PackageType_GameStart");
                    var p = new Backend.GameStartPackage();
                    p.FromByte(pkg);
                    setGameTime((float)p.ServerGameTime);
                    setReady();
                }
                break;
            case Backend.Constant.PackageType_ActionResponse:
                {
                    var package = new Backend.ActionResponsePackage();
                    package.FromByte(pkg);
                    setGameTime((float)package.ServerGameTime);
                }
                break;
            case Backend.Constant.Package_GameEndResult:
                {
                    this.enabled = false;
                    activeWorld = null;
                    ConnectToServer.getInstance().onPkgReceivedHandler -= OnPkgReceived;
                    Debug.Log("Game End Result");
                }
                break;
            case Backend.Constant.Package_TickTimeResponse:
                {
                    tickTimeCount++;
                    TimeSpan rttSpan = DateTime.Now.Subtract(previousTickTimeRequestAt);
                    double rtt = rttSpan.TotalMilliseconds;
                    previousRtt = rtt;
                    sumRtt += rtt;
                    avgRtt = sumRtt / tickTimeCount;

                    Debug.Log($"[TickTime] Ping: {previousRtt} AvgPing: {avgRtt}");

                    var p = new Backend.TickTimeResponsePackage();
                    p.FromByte(pkg);
                    var gt = (float)p.ServerGameTime;
                    if (gt <= lastReceviedGameTime) return;
                    lastReceviedGameTime = gt;
                    setGameTime(gt);
                }
                break;
        }
    }

    public List<MovableWorldObject> moveableObjects = new List<MovableWorldObject>();
    public List<MovableWorldObject> toRemoveMovableObjects = new List<MovableWorldObject>();
    public static void addMovebableObject(IGameWorldUpdatable go) {
        if (activeWorld == null) return;
        activeWorld.moveableObjects.Add(new MovableWorldObject() { worldObject = go });
    }

    public static void removeMovableObject(IGameWorldUpdatable go) {
        if (activeWorld == null) return;

        for(int i = 0; i < activeWorld.moveableObjects.Count; i++){
            if(System.Object.ReferenceEquals(activeWorld.moveableObjects[i].worldObject, go)){
                activeWorld.toRemoveMovableObjects.Add(activeWorld.moveableObjects[i]);
            }
        }       
    }

    public double GetPing(){
        return previousRtt;
    }

    public double GetAvgPing(){
        return avgRtt;
    }
}

public class MovableWorldObject 
{
    public IGameWorldUpdatable worldObject;

    public void Update(float gameTime) 
    {
        if(worldObject != null) {
            worldObject.GameUpdate(gameTime);
        }
    }
}
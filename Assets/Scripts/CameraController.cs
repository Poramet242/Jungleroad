using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    public Transform playerPos; // player
    public Vector3 offSet;
    public float timeMax, timeLeft;
    public Transform targetCam; // newObject 0,0,0
    //public Transform targetPlayer; //Use if limit X
    public int limitCam = 1;
    GridManager calculate;
    public int distanceOfCamera = 4;
    public float centerOfPlayer = 0.25f;

    public bool checkSendOffScreenReport = false;
    [Header("secPerBlock")]
    public float smoothSpeed = 5f;

#if UNITY_EDITOR
    private static bool useOldController = false;
#endif

    private void Start()
    {

#if UNITY_EDITOR
        if (useOldController) {
            Debug.LogWarning(string.Join("\n", 
                "==================================================================",
                "[CameraController] useOldController set as true...",
                "=================================================================="));
            var oldController = this.gameObject.AddComponent<OldCameraController>();
            oldController.playerPos = playerPos;
            oldController.offSet = offSet;
            oldController.smootheFactor = 1f;
            Destroy(this);
        }
#endif

        calculate = GridManager.Instance;
        offSet = new Vector3(calculate.GridToVector(0, distanceOfCamera).x, calculate.GridToVector(0, distanceOfCamera).y + centerOfPlayer, -10f);
        timeMax = distanceOfCamera * secPerBlock;
        timeLeft = timeMax;
        targetCam.position = Vector3.zero - new Vector3(0.25f, 0.125f);
    }
    public void MoveFront()
    {
        timeLeft += secPerBlock;
        if (timeLeft > timeMax)
            timeLeft = timeMax;
    }

    public void MoveBack()
    {
        timeLeft -= secPerBlock;
        if (timeLeft < 0)
            timeLeft = 0;
    }
    [SerializeField] float secPerBlock = 1f;
    private void Update()
    {
        int targetCamXGrid = calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).x;
        int playerXGrid = calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).x;
        int targetCamYGrid = calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).y;
        int playerYGrid = calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).y;
        #region Use x Limit
        //int playerTargetXGrid = calculate.VectorToGrid(targetPlayer.position.x, targetPlayer.position.y).x;
        /*if (calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).x >= limitCam)
        {
            targetPlayer.position = new Vector3(calculate.GridToVector(limitCam, playerYGrid).x, calculate.GridToVector(limitCam, playerYGrid).y, -10);
        }
        else if (calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).x <= -limitCam)
        {
            targetPlayer.position = new Vector3(calculate.GridToVector(-limitCam, playerYGrid).x, calculate.GridToVector(-limitCam, playerYGrid).y, -10);
        }
        else
        {
            targetPlayer.position = new Vector3(playerPos.position.x, playerPos.position.y, -10);
        }

        if (targetCamXGrid != playerTargetXGrid)
        {
            if (calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).x < limitCam && calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).x > -limitCam)
            {
                Vector3 newPos = calculate.GridToVector(calculate.VectorToGrid(targetPlayer.position.x, targetPlayer.position.y).x,
                calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).y);
                targetCam.position = Vector3.Lerp(targetCam.position, newPos + new Vector3(0, 0, -10), Time.deltaTime * 5f);
                timeTest = 0;
            }
            else
            {
                Vector3 newPos = calculate.GridToVector(calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).x,
                calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).y);
                targetCam.position = Vector3.Lerp(targetCam.position, newPos + new Vector3(0, 0, -10), Time.deltaTime * 5f);
                timeTest = 0;
            }
        }
        if (targetCamYGrid < playerYGrid)
        {
            Vector3 newPos = calculate.GridToVector(calculate.VectorToGrid(targetPlayer.position.x, targetPlayer.position.y).x,
            calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).y);
            targetCam.position = Vector3.Lerp(targetCam.position, newPos + new Vector3(0, 0, -10), Time.deltaTime * 5f);
            timeTest = 0;
        }
        else
        {
            targetCam.Translate(new Vector3(0.5f, 0.25f) * Time.deltaTime * secPerBlock); // secPerBlock = 1 | 1 Block = 1 sec , secPerBlock = 0.5 | 1 Block = 2 sec 
        }*/
        #endregion

        if (targetCamXGrid != playerXGrid)
        {
            Vector3 newPos = calculate.GridToVector(calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).x,
            calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).y);
            float vX = targetCam.position.x - calculate.GridToVector(targetCamXGrid, targetCamYGrid).x;
            float vY = targetCam.position.y - calculate.GridToVector(targetCamXGrid, targetCamYGrid).y;
            targetCam.position = Vector3.Lerp(targetCam.position, newPos + new Vector3(vX, vY, -10), Time.deltaTime * smoothSpeed);
        }
        if (targetCamYGrid < playerYGrid)
        {
            Vector3 newPos = calculate.GridToVector(calculate.VectorToGrid(targetCam.position.x, targetCam.position.y).x,
            calculate.VectorToGrid(playerPos.position.x, playerPos.position.y).y);
            targetCam.position = Vector3.Lerp(targetCam.position, newPos + new Vector3(0, 0, -10), Time.deltaTime * smoothSpeed);
            timeLeft = timeMax;
        }
        targetCam.Translate(new Vector3(0.5f, 0.25f) * Time.deltaTime / secPerBlock); // secPerBlock = 1 | 1 Block = 1 sec , secPerBlock = 0.5 | 1 Block = 2 sec 
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
        else
        {
            //dead
            if (!checkSendOffScreenReport)
            {
                double diffInSeconds = (DateTime.Now - DataManager.Instance.gameStartTime).TotalSeconds;
                Backend.BackendComm.instance.SendOffScreenReport(diffInSeconds);
                checkSendOffScreenReport = true;
                PlayerMenager.instance.ShowAniamtionDead(3);
                ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            }
        }
        transform.position = targetCam.position + offSet;
    }

    public class OldCameraController : MonoBehaviour
    {
        public Transform playerPos;
        public Vector3 offSet;
        [Range(1, 10)]
        public float smootheFactor;

        private void FixedUpdate()
        {
            Vector3 playerPosition = playerPos.position + offSet;
            Vector3 smoothPos = Vector3.Lerp(transform.position, playerPosition, smootheFactor * Time.fixedDeltaTime);
            if (transform.position.x < smoothPos.x || transform.position.y < smoothPos.y)
                transform.position = smoothPos;
        }
    }

}
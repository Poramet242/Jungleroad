using System;
using System.Collections;
using System.Collections.Generic;
using Backend;
using UnityEngine;
//using DG.Tweening;

public class PlayerMenager : MonoBehaviour
{
    public static PlayerMenager instance;
    [SerializeField] Vector2Int myGrid;
    [SerializeField] Vector2Int targetGrid;

    private bool checkDeadToCar;
    private bool checkDeadToWater;
    private bool checkDeadToCamera;
    private bool checkDeadToTrain;
    private bool checkAniamtion;

    public bool ShowCheckAnimation
    {
        get { return checkAniamtion; }
        set { checkAniamtion = value; }
    }
    [Header("MoveCurve")]
    [SerializeField] private AnimationCurve moveCurve;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject smokeParticle;
    [SerializeField] GameObject waterParticle;
    [SerializeField] GameObject impactParticle;
    [SerializeField] Transform spawnParticlePos;
    [SerializeField] Transform parentEffect;
    float t;
    bool canMove = true;
    AudioSource audioSource;

    [SerializeField]
    private GameObject playerEffect;

    [SerializeField] Animator newAnim;

    [SerializeField] float speed;
    [Range(0.1f, 0.9f)]
    public float delayInput;

    [Tooltip(
        "When Input Offline set as [True], input will be not sent to server,\n" +
        "then it does not wait for any action response from server\n" +
        "please note that this flag has effect ONLY in EDITOR mode. see Awake function")]
    Vector2 startTouchPos;
    int detectRange = 100;
    bool alreadyTouch;
    [SerializeField] bool inputOffline = false;

    Dictionary<int, Action<ActionResponsePackage>> watchingInputSequenceNumber = new Dictionary<int, Action<ActionResponsePackage>>();

    public CharacterEffectController CEC;

    void Awake()
    {
        if (instance == null)
            instance = this;
#if !UNITY_EDITOR
        inputOffline = false;
#endif
    }

    void Start()
    {
        GridDatabase.Instance.StartAddPlayerToArray(Vector2Int.zero);
        const string logTag = "[PlayerManager::Start]";

        myGrid = Vector2Int.zero;

        t = 0;
        if (centorPivot == null)
            centorPivot = transform.GetChild(0);
        audioSource = gameObject.GetComponent<AudioSource>();
        newAnim.runtimeAnimatorController = DataTransferScene.Instance.animatorController[DataTransferScene.Instance.selectNumber];

        ConnectToServer.getInstance().onReceiveActionResponse += HandleActionResponse;
        Debug.Log($"{logTag} register onReceiveActionResponse handler... done");


        if (ListEffectsManager.instance.effect[DataTransferScene.Instance.selectNumber] != null)
        {
            playerEffect = Instantiate(ListEffectsManager.instance.effect[DataTransferScene.Instance.selectNumber]);
            playerEffect.transform.parent = parentEffect.transform;
            playerEffect.transform.localPosition = Vector3.forward * -0.1f;
            CEC = playerEffect.GetComponent<CharacterEffectController>();
        }

#if UNITY_EDITOR
        if (inputOffline)
        {
            UnityEditor.EditorUtility.DisplayDialog("Warning", "PlayerManager.inputOffline is set as true", "Ok");
        }
#endif
    }

    void OnDestroy()
    {
        const string logTag = "[PlayerManager::OnDestroy]";
        ConnectToServer.getInstance().onReceiveActionResponse -= HandleActionResponse;
        Debug.Log($"{logTag} unregister onReceiveActionResponse handler... done");
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (canMove)
            HandleInput();
        float step = speed * Time.deltaTime; // calculate distance to move
        if (!GridDatabase.Instance.onLog)
        {
            transform.position = Vector3.MoveTowards(transform.position, GridManager._instance.GridToVector(GridDatabase.Instance.moveToGrid.x, GridDatabase.Instance.moveToGrid.y), step);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, GridDatabase.Instance.playerTransform.position, step);
            /*if (GridManager.Instance.VectorToGrid(transform.position.x, transform.position.y).x < -4 || GridManager.Instance.VectorToGrid(transform.position.x, transform.position.y).x > 4)
            {
                Debug.LogError("Dead by waterfall");
                ShowAniamtionDead(2);
            }*/
        }
    }
    private void SendInput(byte input, Action<ActionResponsePackage> actionRespCB = null)
    {
        // if inputOffline set, we mimic the actionResponse like the server returns response with canMove = true
        if (inputOffline)
        {
            actionRespCB(new ActionResponsePackage()
            {
                CanMove = true,
            });
            return;
        }
        double diffInSeconds = (DateTime.Now - DataManager.Instance.gameStartTime).TotalSeconds;
        var seq = Backend.BackendComm.instance.SendInput(input, diffInSeconds); // Sendbyte & delaytime
        watchingInputSequenceNumber[seq] = actionRespCB;
        Debug.Log($"<COLOR=White> SendInput: watching Input Seq: {seq} at clientTime: {diffInSeconds} gameTime: {GameWorld.getGameTime()} </COLOR>");
    }
    [SerializeField] private bool useMobileInput;
    void HandleInput()
    {
        if (!useMobileInput)
        {
            #region MouseInput
            /*if (alreadyTouch == false && Input.GetMouseButtonDown(0))
            {
                startTouchPos = Input.mousePosition;
                alreadyTouch = true;
            }

            if (alreadyTouch)
            {
                if (Input.mousePosition.y >= startTouchPos.y + detectRange)
                {
                    alreadyTouch = false;
                    print("Swipe up");
                    SendInput(Backend.Constant.Input_Forward, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
                else if (Input.mousePosition.x <= startTouchPos.x - detectRange)
                {
                    alreadyTouch = false;
                    print("Swipe left");
                    SendInput(Backend.Constant.Input_Left, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, true, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
                else if (Input.mousePosition.x >= startTouchPos.x + detectRange)
                {
                    alreadyTouch = false;
                    print("Swipe right");
                    SendInput(Backend.Constant.Input_Right, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_DOWN, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
                else if (Input.mousePosition.y <= startTouchPos.y - detectRange)
                {
                    alreadyTouch = false;
                    print("Swipe down");
                    SendInput(Backend.Constant.Input_Backward, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, true, AnimationController.AnimationListState.JUMP_DOWN, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
            }

            if (alreadyTouch && Input.GetMouseButtonUp(0))
            {
                alreadyTouch = false;
            }*/
            #endregion
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                SendInput(Backend.Constant.Input_Left, (actionResp) =>
                {
                    var nextLane = actionResp.CurrentLane;
                    var nextBlockIndex = actionResp.CurrentIndex;
                    MoveCalculate(nextLane, nextBlockIndex, true, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                    InputResult(actionResp);
                });
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                SendInput(Backend.Constant.Input_Right, (actionResp) =>
                {
                    var nextLane = actionResp.CurrentLane;
                    var nextBlockIndex = actionResp.CurrentIndex;
                    MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_DOWN, actionResp.MoveResult);
                    InputResult(actionResp);
                });
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SendInput(Backend.Constant.Input_Forward, (actionResp) =>
                {
                    var nextLane = actionResp.CurrentLane;
                    var nextBlockIndex = actionResp.CurrentIndex;
                    MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                    InputResult(actionResp);
                });
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SendInput(Backend.Constant.Input_Backward, (actionResp) =>
                {
                    var nextLane = actionResp.CurrentLane;
                    var nextBlockIndex = actionResp.CurrentIndex;
                    MoveCalculate(nextLane, nextBlockIndex, true, AnimationController.AnimationListState.JUMP_DOWN, actionResp.MoveResult);
                    InputResult(actionResp);
                });
            }
            return;
        }
        else
        {
            if (alreadyTouch == false && Input.touchCount > 0
                && Input.touches[0].phase == TouchPhase.Began)
            {
                startTouchPos = Input.touches[0].position;
                alreadyTouch = true;
            }

            if (alreadyTouch)
            {
                if (Input.touches[0].position.y >= startTouchPos.y + detectRange)
                {
                    alreadyTouch = false;
                    //print("Swipe up");
                    SendInput(Backend.Constant.Input_Forward, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
                else if (Input.touches[0].position.x <= startTouchPos.x - detectRange)
                {
                    alreadyTouch = false;
                    //print("Swipe left");
                    SendInput(Backend.Constant.Input_Left, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, true, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
                else if (Input.touches[0].position.x >= startTouchPos.x + detectRange)
                {
                    alreadyTouch = false;
                    //print("Swipe right");
                    SendInput(Backend.Constant.Input_Right, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_DOWN, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
                else if (Input.touches[0].position.y <= startTouchPos.y - detectRange)
                {
                    alreadyTouch = false;
                    //print("Swipe down");
                    SendInput(Backend.Constant.Input_Backward, (actionResp) =>
                    {
                        var nextLane = actionResp.CurrentLane;
                        var nextBlockIndex = actionResp.CurrentIndex;
                        MoveCalculate(nextLane, nextBlockIndex, true, AnimationController.AnimationListState.JUMP_DOWN, actionResp.MoveResult);
                        InputResult(actionResp);
                    });
                }
            }
            if (alreadyTouch && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
            {
                alreadyTouch = false;
                //print("Swipe up");
                SendInput(Backend.Constant.Input_Forward, (actionResp) =>
                {
                    var nextLane = actionResp.CurrentLane;
                    var nextBlockIndex = actionResp.CurrentIndex;
                    MoveCalculate(nextLane, nextBlockIndex, false, AnimationController.AnimationListState.JUMP_UP, actionResp.MoveResult);
                    InputResult(actionResp);
                });
            }
            return;
        }
    }
    IEnumerator MoveDelay()
    {
        yield return new WaitForSeconds(delayInput);
        canMove = true;
    }
    void MoveCalculate(int nextLane, int nextBlockIndex, bool flip, string animationState, byte moveResult)
    {
        #region MoveCalculateOld
        /*
        Vector2Int gridTarget = myGrid + gridInt;
        if (!IsBlocked(gridTarget))
        {
            targetGrid = myGrid + gridInt;
            t = 0;
            spriteRenderer.flipX = flip;
            AnimationController.Instance.ChangeAnimationState(animationState);
            SoundManager.Instance.PlaySound(SoundManager.Sound.playerMovement, audioSource);
        }
        Vector2Int logTarget = GridManager._instance.VectorToGrid(transform.position.x, transform.position.y) + gridInt;
        if (log != null && !IsLog(logTarget))
        {
            gridTarget = GridManager._instance.VectorToGrid(transform.position.x, transform.position.y) + gridInt;
            if (!IsBlocked(gridTarget))
            {
                myGrid = GridManager._instance.VectorToGrid(transform.position.x, transform.position.y);
                targetGrid = myGrid + gridInt;
                t = 0;
                spriteRenderer.flipX = flip;
                AnimationController.Instance.ChangeAnimationState(animationState);
                log = null;
            }
        }
        else if (IsLog(logTarget))
        {
            t = 0;
        }
        if (isWater(logTarget))
        {
            if (IsWaterButOnOB(logTarget))
            {
                myGrid = GridManager._instance.VectorToGrid(transform.position.x, transform.position.y);
                targetGrid = myGrid + gridInt;
                t = 0;
                spriteRenderer.flipX = flip;
                AnimationController.Instance.ChangeAnimationState(animationState);
                SoundManager.Instance.PlaySound(SoundManager.Sound.jumpOnLog, audioSource);
            }
            // Dead
            else
            {
                myGrid = GridManager._instance.VectorToGrid(transform.position.x, transform.position.y);
                targetGrid = myGrid + gridInt;
                t = 1;
                Instantiate(waterParticle, spawnParticlePos);
                spriteRenderer.enabled = false;

                // SoundManager.Instance.PlaySound(SoundManager.Sound.die_water, audioSource);
                // ScoreManager.Instance.gameoverPanel.SetActive(true);
                //this.enabled = false;
            }
        }
        ScoreManager.Instance.ScorePlus(myGrid.y + gridInt.y);
        canMove = false;
        StartCoroutine(nameof(MoveDelay));
        */
        #endregion

        Vector2Int nextToVector = new Vector2Int(nextBlockIndex, nextLane);

        if (moveResult == 6)
        {
            GridDatabase.Instance.GatorRive = true;
        }
        GridDatabase.Instance.PlayerMove(nextToVector);


        spriteRenderer.flipX = flip;
        AnimationController.Instance.ChangeAnimationState(animationState);
        SoundManager.Instance.PlaySound(SoundManager.Sound.playerMovement, audioSource);

        ScoreManager.Instance.ScorePlus(GridDatabase.Instance.moveToGrid.y);
        GenerateMapRepeat.Instance.GenerateMap(GridDatabase.Instance.moveToGrid.y);
        canMove = false;
        StartCoroutine(nameof(MoveDelay));
    }

    IEnumerator Dead(float delayTime, GameObject particle)
    {
        yield return new WaitForSeconds(delayTime);
    }

    [Header("Raycast")]
    [SerializeField] Transform centorPivot;
    Vector2 rayDirection;
    float rayDistance;
    bool IsBlocked(Vector2Int target)
    {
        rayDirection = GridManager._instance.GridToVector(target.x, target.y) - GridManager._instance.GridToVector(myGrid.x, myGrid.y);
        rayDistance = 0.01f;

        RaycastHit2D hit = Physics2D.Raycast(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection, rayDistance);
        //If something was hit.
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Tree"))
            {
                Debug.DrawRay(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection * rayDistance, Color.red, 5f);
                return true;
            }
        }
        Debug.DrawRay(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection * rayDistance, Color.green, 5f);
        return false;
    }

    [SerializeField] GameObject log;
    bool IsLog(Vector2Int target)
    {
        rayDirection = GridManager._instance.GridToVector(target.x, target.y) - GridManager._instance.GridToVector(myGrid.x, myGrid.y);
        rayDistance = 0.01f;
        RaycastHit2D hit = Physics2D.Raycast(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection, rayDistance);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Log"))
            {
                //Debug.DrawRay(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection * rayDistance, Color.blue, 5f);
                log = hit.collider.gameObject;
                return true;
            }
        }
        //Debug.DrawRay(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection * rayDistance, Color.red, 5f);
        return false;
    }
    bool IsWaterButOnOB(Vector2Int target)
    {
        rayDirection = GridManager._instance.GridToVector(target.x, target.y) - GridManager._instance.GridToVector(myGrid.x, myGrid.y);
        rayDistance = 0.01f;
        RaycastHit2D[] hit = Physics2D.RaycastAll(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection, rayDistance);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit.Length > 1 && hit[i].collider.CompareTag("Log"))
            {
                foreach (var item in hit)
                {
                    if (item.collider.CompareTag("Lily") || item.collider.CompareTag("Log"))
                    {
                        return true;
                    }
                }
            }
        }
        return false;

        /*if (hit.Length > 1 && hit[0].collider.CompareTag("Water"))
        {
            foreach (var item in hit)
            {
                if (item.collider.CompareTag("Lily") || item.collider.CompareTag("Log"))
                {
                    return true;
                }
            }
        }
        else if (hit.Length > 1 && hit[1].collider.CompareTag("Water"))
        {
            foreach (var item in hit)
            {
                if (item.collider.CompareTag("Lily") || item.collider.CompareTag("Log"))
                {
                    return true;
                }
            }
        }
        return false;*/
    }
    bool isWater(Vector2Int target)
    {
        rayDirection = GridManager._instance.GridToVector(target.x, target.y) - GridManager._instance.GridToVector(myGrid.x, myGrid.y);
        rayDistance = 0.01f;
        RaycastHit2D[] hit = Physics2D.RaycastAll(GridManager._instance.GridToVector(target.x, target.y) + new Vector2(centorPivot.localPosition.x, centorPivot.localPosition.y), rayDirection, rayDistance);
        foreach (var item in hit)
        {
            if (item.collider.CompareTag("Water"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        #region Dead
        /*// Dead
        if (collision.CompareTag("OB") || collision.CompareTag("Train"))
        {
            AnimationController.Instance.ChangeAnimationState(AnimationController.AnimationListState.DIE_HIT);
            if (AnimationController.Instance.isForward)
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
                spriteRenderer.flipY = !spriteRenderer.flipY;
            }
            else
            {
                spriteRenderer.flipX = !spriteRenderer.flipX;
            }
            if (collision.CompareTag("OB"))
                Instantiate(smokeParticle, spawnParticlePos);
            if (collision.CompareTag("Train"))
                Instantiate(impactParticle, spawnParticlePos);
            GetComponent<Collider2D>().enabled = false;
            SoundManager.Instance.PlaySound(SoundManager.Sound.die_hit, audioSource);
            Debug.Log($"Dead in client [gameTime: {GameWorld.getGameTime()}] [cause: OB|Train]");

            UIManager.Instance.ShowDeadInClient = true;
            UIManager.Instance.CallDead();
            this.enabled = false;
        }
        if (collision.CompareTag("WaterDead"))
        {
            Instantiate(waterParticle, spawnParticlePos);
            spriteRenderer.enabled = false;
            SoundManager.Instance.PlaySound(SoundManager.Sound.die_water, audioSource);
            gameObject.transform.SetParent(null);
            Debug.Log($"Dead in client [gameTime: {GameWorld.getGameTime()}] [cause: water]");
            UIManager.Instance.ShowDeadInClient = true;
            UIManager.Instance.CallDead();
            this.enabled = false;
        }
        */
        #endregion
        if (collision.CompareTag("MileStone"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Sound.milestone, audioSource);
            collision.gameObject.SetActive(false);
        }
        if (collision.CompareTag("Coin"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Sound.coin, audioSource);
            ScoreManager.Instance.coinesPlus();
            collision.gameObject.SetActive(false);
        }
    }
    /*private void SentAllData()
    {
        object[] dataStorage = new object[4];
        dataStorage[0] = DataManager.Instance.playerData.packageType;
        dataStorage[1] = DataManager.Instance.playerData.packageNo;
        dataStorage[2] = DataManager.Instance.playerData.clientTime;
        dataStorage[3] = DataManager.Instance.playerData.inputValue;

        foreach (var item in dataStorage)
        {
            print(item);
        }
    }*/

    void HandleActionResponse(ActionResponsePackage actionResp)
    {
        if (watchingInputSequenceNumber.Count == 0) return;

        var seq = actionResp.ActionSeqNumber;
        if (!watchingInputSequenceNumber.ContainsKey(seq)) return;

        var cb = watchingInputSequenceNumber[seq];
        if (cb == null) return;

        //TODO: chanyutl - still have the better ideas to handle the actionResponse
        // + make it optimistic, by letting player move before and check the action response later
        // + wait until client get the action response for the input (input sequenceNumber match with the actionReponse's inputSequenceNubmber) 
        cb(actionResp);
    }
    public void ShowAniamtionDead(int numOfDaed)
    {
        if (ShowCheckAnimation)
        {
            return;
        }
        Debug.Log($"<COLOR=Red> Show Animation Dead </COLOR>");
        if (API.isAlphaTest)
        {
            UIManager.Instance.CheckEndGame = true;
        }
        if (numOfDaed == 1)
        {
            Debug.Log($"<COLOR=Green> Caes : 1 Dead by Object in land </COLOR>");
            DeadAnimation();
            SoundManager.Instance.PlaySound(SoundManager.Sound.die_hit, audioSource);
            if (CameraController.Instance != null)
            {
                CameraController.Instance.enabled = false;
            }
            if (playerEffect != null)
            {
                playerEffect.gameObject.SetActive(false);
            }
            UIManager.Instance.CallDead();
            Debug.LogError($"Dead in client [gameTime: {GameWorld.getGameTime()}] [cause: OB|In Land]");
            ShowCheckAnimation = true;
        }
        else if (numOfDaed == 2)
        {
            Debug.Log("$<COLOR=Green> Caes : 2 Dead by Water in land </COLOR>");
            Instantiate(waterParticle, spawnParticlePos);
            spriteRenderer.enabled = false;
            SoundManager.Instance.PlaySound(SoundManager.Sound.die_water, audioSource);
            gameObject.transform.SetParent(null);
            if (CameraController.Instance != null)
            {
                CameraController.Instance.enabled = false;
            }
            if (playerEffect != null)
            {
                playerEffect.gameObject.SetActive(false);
            }

            UIManager.Instance.CallDead();
            Debug.LogError($" in client [gameTime: {GameWorld.getGameTime()}] [cause: OB|Water]");
            ShowCheckAnimation = true;
        }
        else if (numOfDaed == 3)
        {
            Debug.Log($"<COLOR=Green> Caes : 1 Dead by Object in land </COLOR>");
            DeadAnimation();
            SoundManager.Instance.PlaySound(SoundManager.Sound.sad, audioSource);
            if (CameraController.Instance != null)
            {
                CameraController.Instance.enabled = false;
            }

            if (playerEffect != null)
            {
                playerEffect.gameObject.SetActive(false);
            }
            UIManager.Instance.CallDead();
            Debug.LogError($"Dead in client [gameTime: {GameWorld.getGameTime()}] [cause: Camera]");
            ShowCheckAnimation = true;
        }
    }
    public void DeadAnimation()
    {
        AnimationController.Instance.ChangeAnimationState(AnimationController.AnimationListState.DIE_HIT);
        if (AnimationController.Instance.isForward)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
        else
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
        Instantiate(smokeParticle, spawnParticlePos);
        Instantiate(impactParticle, spawnParticlePos);

    }
    public void InputResult(ActionResponsePackage arp)
    {
        const string logTag = "[HandleInput]";
        if (!arp.CanMove)
        {
            Debug.LogWarning($"{logTag} Input_Backward canMove: false");
        }
        CEC?.ChangeAnimationState(2);
        if (arp.MoveResult == Constant.MoveResult_Drown)
        {
            Debug.LogError($"Dead in server [gameTime: {GameWorld.getGameTime()}]");
            ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            ShowAniamtionDead(2);
        }
        else if (arp.MoveResult == Constant.MoveResult_Hit)
        {
            Debug.LogError($"Dead in server [gameTime: {GameWorld.getGameTime()}]");
            ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            ShowAniamtionDead(1);
        }
        else if (arp.MoveResult == Constant.MoveResult_HIT_GATOR)
        {
            Debug.LogError($"Dead in server [gameTime: {GameWorld.getGameTime()}]");
            ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            ShowAniamtionDead(1);
        }
        else if (arp.MoveResult == Constant.MoveResult_HIT_TRAIN)
        {
            Debug.LogError($"Dead in server [gameTime: {GameWorld.getGameTime()}]");
            ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            ShowAniamtionDead(1);
        }
        else if (arp.MoveResult == Constant.MoveResult_OffscreenSide)
        {
            Debug.LogError($"Dead in server [gameTime: {GameWorld.getGameTime()}]");
            ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            ShowAniamtionDead(2);
        }
        else if (arp.MoveResult == Constant.MoveResult_OffscreenBack)
        {
            Debug.LogError($"Dead in server [gameTime: {GameWorld.getGameTime()}]");
            ConnectToServer.getInstance().SetGameState(ConnectToServer.GameState.Dead);
            ShowAniamtionDead(3);
        }
        else
        {
            Debug.Log("<COLOR=Green> ARP </COLOR>");
        }
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(255f, 0, 217f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0), transform.position + new Vector3(0, 0.5f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0), transform.position);
    }
#endif
}

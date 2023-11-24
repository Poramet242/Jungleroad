using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridDatabase : MonoBehaviour
{
    public static GridDatabase Instance;

    [SerializeField] int maxGridX = 9, maxGridY = 9999;
    private DataType[,] gridArray;
    [SerializeField] int playZoneX = 9, maxMinusY = 4;
    [SerializeField]
    AudioSource audioSource;
    public enum DataType
    {
        None = 0,
        Player = 1,
        Tree = 2,
        Log = 3,
        OB = 4,
        Crocodile_b = 5,
        Water = 6,
        Crocodile_h = 7,
        Train = 8
    }

    //public List<Vector2Int> carList; // Recieved position & Change position
    //public List<Vector2Int> carSorted; // Use to for loop to find y and break

    public List<LogScript> logDatas = new List<LogScript>();
    public List<CarScript> carDatas = new List<CarScript>();
    public List<TrainScript> trainDatas = new List<TrainScript>(); // GridDatabase.Instance.trainDatas.Add(this);
    public List<GatorScript> gatorDatas = new List<GatorScript>(); // GridDatabase.Instance.gatorDatas.Add(this);

    [SerializeField] private Vector2Int playerGrid;
    public Vector2Int moveToGrid;

    public Transform playerTransform; // Onlog || OnGator
    public PlayerMenager playerMenager;
    public bool onLog;
    public bool GatorRive;

    private float DelayCount = 0.1794375309f;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        gridArray = new DataType[maxGridX, maxGridY];
    }

    [SerializeField]
    private CameraController cameraCon;
    public void StartAddPlayerToArray(Vector2Int targetGrid) // Use Function On Player Start
    {
        Vector2Int newGrid = new Vector2Int(playerGrid.x + targetGrid.x +
            Mathf.FloorToInt(playZoneX / 2), playerGrid.y + targetGrid.y + maxMinusY);
        playerGrid = newGrid;
        gridArray[newGrid.x, newGrid.y] = 0;
        Debug.Log(newGrid);

        /*[0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],
          [0,1,2,3,4,5,6,7,8],*/
    }
    private void Start()
    {
        InvokeRepeating(nameof(ObjectMove), 0, 0.2f);
    }
    private void Update()
    {
        if (onLog)
        {
            //print(playerTransform.position);

            playerGrid = GridToArray(GridManager.Instance.VectorToGrid(playerTransform.position.x, playerTransform.position.y));
        }
    }
    #region PlayerMove
    public void PlayerMove(Vector2Int movePos) // Call for move
    {
        Vector2Int targetGrid = movePos;

        //print(targetGrid.x);

        if (gridArray[targetGrid.x, targetGrid.y] != DataType.Tree)
        {
            if (targetGrid.y - 4 > moveToGrid.y)
            {
                cameraCon.MoveFront();
            }
            else if (targetGrid.y - 4 < moveToGrid.y)
            {
                cameraCon.MoveBack();
            }
            PlayerChangeGrid(targetGrid);
        }
        else
        {
            print("Block by tree");
        }
    }
    void PlayerChangeGrid(Vector2Int targetGrid)
    {
        moveToGrid = new Vector2Int(targetGrid.x - Mathf.FloorToInt(playZoneX / 2), targetGrid.y - maxMinusY);

        CheckAfterMove(targetGrid); // Maybe delay from sec for move
    }
    void CheckAfterMove(Vector2Int targetGrid) // Change bool onLog
    {
        DataType backupDataType = DataType.None;
        // Convert To GridToArray
        if (GridInArray(targetGrid.x))
        {
            backupDataType = gridArray[targetGrid.x, targetGrid.y];
            if (backupDataType != DataType.None)
            {
                backupDataType = DataType.None;
            }
        }

        Debug.Log($"<COLOR=Cyan> Player jump at !</COLOR>" + targetGrid);
        Debug.Log($"<COLOR=Blue> That block is !</COLOR>" + gridArray[targetGrid.x, targetGrid.y]);
        StartCoroutine(DelayCheck(targetGrid, backupDataType));
    }
    #endregion
    public void PlayerChangeData(Vector2Int oldGrid, Vector2Int newGrid, DataType oldDataType, DataType newDatatype)
    {
        gridArray[oldGrid.x, oldGrid.y] = oldDataType;
        Debug.Log("<COLOR=Magenta> oldGrid is !</COLOR>" + oldGrid);
        Debug.Log("<COLOR=Magenta> oldGrid type is !</COLOR>" + oldDataType);
        gridArray[newGrid.x, newGrid.y] = newDatatype;
        Debug.Log("<COLOR=White> newGrid is !</COLOR>" + newGrid);
        Debug.Log("<COLOR=White> newGrid type is !</COLOR>" + newDatatype);
    }

    public void AddObjectToArray(Vector2Int targetGrid, int datatype)
    {
        // switch dataType
        gridArray[GridToArray(targetGrid).x, GridToArray(targetGrid).y] = (DataType)datatype;
    }
    public void RemoveobjectFromArray(Vector2Int targetGrid)
    {
        gridArray[GridToArray(targetGrid).x, GridToArray(targetGrid).y] = DataType.None;
    }
    public void SetTree(Vector2Int targetGrid)
    {
        Vector2Int target = GridToArray(targetGrid);
        gridArray[target.x, target.y] = DataType.Tree;
    }
    void ObjectMove() //Change To ObjectMove() {InRen(Log); InRen(Car);, InRen(Gator);}
    {
        InRendererLine(DataType.Log);
        InRendererLine(DataType.OB);
        InRendererLine(DataType.Train);
        InRendererLine(DataType.Crocodile_h);
    }
    public void InRendererLine(DataType dataType) // ReCheck*** for break
    {
        switch (dataType)
        {
            case DataType.Log:
                for (int i = 0; i < logDatas.Count; i++)
                {
                    if (logDatas[i] == null)
                    {
                        logDatas.RemoveAt(0); // Delete if bug
                        continue;
                    }
                    int objectGridY = GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y).y;
                    if (objectGridY < playerGrid.y - 11 - maxMinusY)
                    {
                        continue;
                    }
                    if (objectGridY >= playerGrid.y - 10 - maxMinusY && objectGridY <= playerGrid.y + 10 - maxMinusY)
                    {
                        Vector2Int newV2 = GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y);
                        if (logDatas[i].logPos == -9 || logDatas[i].logPos == 17)
                        {
                            int num = i - logDatas[i].obInLane + 1;
                            if (num >= 0)
                            {
                                if (logDatas[num].logPos == -9 || logDatas[num].logPos == 17)
                                {
                                    if (logDatas[num].dis > 0 && logDatas[num].logPos == 17 || logDatas[num].dis < 0 && logDatas[num].logPos == -9)
                                    {
                                        ObjectChangeData(logDatas[i].dis, newV2, DataType.Water, dataType, false);
                                    }
                                    else
                                    {
                                        ObjectChangeData(logDatas[i].dis, newV2, DataType.Water, dataType, true);
                                    }
                                }
                                else
                                {
                                    ObjectChangeData(logDatas[i].dis, newV2, DataType.Water, dataType, true);
                                }
                            }
                            else
                            {
                                ObjectChangeData(logDatas[i].dis, newV2, DataType.Water, dataType, true);
                            }
                        }
                        else
                        {
                            ObjectChangeData(logDatas[i].dis, newV2, DataType.Water, dataType, true);
                        }
                        continue;
                    }
                }
                break;
            case DataType.OB:
                for (int i = 0; i < carDatas.Count; i++)
                {
                    if (carDatas[i] == null)
                    {
                        carDatas.RemoveAt(0); // Delete if bug
                        continue;
                    }
                    int objectGridY = GridManager.Instance.VectorToGrid(carDatas[i].transform.position.x, carDatas[i].transform.position.y).y;
                    if (objectGridY < playerGrid.y - 11 - maxMinusY)
                    {
                        continue;
                    }
                    if (objectGridY >= playerGrid.y - 10 - maxMinusY && objectGridY <= playerGrid.y + 10 - maxMinusY) // -1 as player
                    {
                        Vector2Int newV2 = GridManager.Instance.VectorToGrid(carDatas[i].transform.position.x, carDatas[i].transform.position.y);
                        //print(newV2);
                        if (carDatas[i].carPos == -9 || carDatas[i].carPos == 17)
                        {
                            int num = i - carDatas[i].obInLane + 1;
                            if (num >= 0)
                            {
                                if (carDatas[num].carPos == -9 || carDatas[num].carPos == 17)
                                {
                                    if (carDatas[num].dir > 0 && carDatas[num].carPos == 17 || carDatas[num].dir < 0 && carDatas[num].carPos == -9)
                                    {
                                        ObjectChangeData(carDatas[i].dir, newV2, DataType.None, dataType, false);
                                    }
                                    else
                                    {
                                        ObjectChangeData(carDatas[i].dir, newV2, DataType.None, dataType, true);
                                    }
                                }
                                else
                                {
                                    ObjectChangeData(carDatas[i].dir, newV2, DataType.None, dataType, true);
                                }
                            }
                            else
                            {
                                ObjectChangeData(carDatas[i].dir, newV2, DataType.None, dataType, true);
                            }
                        }
                        else
                        {
                            ObjectChangeData(carDatas[i].dir, newV2, DataType.None, dataType, true);
                        }
                        continue;
                    }
                    /*else
                    {
                        break;
                    }*/
                }
                break;
            case DataType.Crocodile_h:
                for (int i = 0; i < gatorDatas.Count; i++)
                {
                    if(gatorDatas[i] == null)
                    {
                        gatorDatas.RemoveAt(0); // Delete if bug
                        continue;
                    }
                    int objectGridY = GridManager.Instance.VectorToGrid(gatorDatas[i].transform.position.x, gatorDatas[i].transform.position.y).y;
                    if (objectGridY < playerGrid.y - 11 - maxMinusY)
                    {
                        continue;
                    }
                    if (objectGridY >= playerGrid.y - 10 - maxMinusY && objectGridY <= playerGrid.y + 10 - maxMinusY) // -1 as player
                    {
                        Vector2Int newV2 = GridManager.Instance.VectorToGrid(gatorDatas[i].transform.position.x, gatorDatas[i].transform.position.y);
                        //print(newV2);
                        ObjectSizeChangeData(gatorDatas[i].dis, newV2, dataType);
                        continue;
                    }
                    /*else
                    {
                        break;
                    }*/
                }
                break;
            case DataType.Train:
                for (int i = 0; i < trainDatas.Count; i++)
                {
                    if(trainDatas[i] == null)
                    {
                        trainDatas.RemoveAt(0); // Delete if bug
                        continue;
                    }
                    int objectGridY = GridManager.Instance.VectorToGrid(trainDatas[i].transform.position.x, trainDatas[i].transform.position.y).y;
                    if (objectGridY < playerGrid.y - 11 - maxMinusY)
                    {
                        continue;
                    }
                    if (objectGridY >= playerGrid.y - 10 - maxMinusY && objectGridY <= playerGrid.y + 10 - maxMinusY) // -1 as player
                    {
                        Vector2Int newV2 = GridManager.Instance.VectorToGrid(trainDatas[i].transform.position.x, trainDatas[i].transform.position.y);
                        //print(newV2);
                        ObjectSizeChangeData(trainDatas[i].movDir, newV2, dataType);
                        continue;
                    }
                    /*else
                    {
                        break;
                    }*/
                }
                break;
        }
    }
    public void ObjectChangeData(int dir, Vector2Int newGrid, DataType oldDatatype, DataType newDatatype, bool changeOld)
    {
        Vector2Int oldGrid;
        switch (newDatatype)
        {
            case DataType.Log:
                break;
            case DataType.OB:
                //print(GridInArray(newGrid.x));
                if (GridInArray(newGrid.x) && gridArray[GridToArray(newGrid).x, GridToArray(newGrid).y] == DataType.Player) // Change To DataType (newDatatype == DataType.Player)
                {
                    //Debug.Log("<COLOR=Magenta> CrashByCar !</COLOR>");
                    PlayerMenager.instance.ShowAniamtionDead(1);
                }
                break;
            default:
                break;
        }
        if (dir == 1)
            oldGrid = new Vector2Int(-1, 0) + newGrid;
        else
            oldGrid = new Vector2Int(1, 0) + newGrid;
        if (changeOld)
        {
            if (GridInArray(oldGrid.x))
            {
                Vector2Int g = GridToArray(oldGrid);
                if (gridArray[g.x, g.y] != DataType.Player || onLog)
                {
                    gridArray[g.x, g.y] = oldDatatype;
                    //print($"Change" + g + "=" + gridArray[g.x, g.y]);
                }
            }
        }
        if (GridInArray(newGrid.x))
        {
            Vector2Int g = GridToArray(newGrid);
            if (gridArray[g.x, g.y] != DataType.Player || onLog)
            {
                gridArray[g.x, g.y] = newDatatype; // if(new is log = log || old is none = log)
                //print($"Change" + g + "=" + gridArray[g.x, g.y]);
            }
        }
    }
    /* public void ObjectSizeChangeData(int dir, Vector2Int newGrid, DataType dataType)
     {
         switch (dataType)
         {
             case DataType.Crocodile_h:
                 Vector2Int h = newGrid;
                 Vector2Int b = new Vector2Int(dir * -1, 0) + newGrid;
                 Vector2Int oldGrid = new Vector2Int(dir * -2, 0) + newGrid;
                 if (GridInArray(h.x))
                 {
                     if (gridArray[h.x, h.y] != DataType.Player)
                         gridArray[h.x, h.y] = DataType.Crocodile_h;

                     if (gridArray[b.x, b.y] != DataType.Player)
                         gridArray[b.x, b.y] = DataType.Crocodile_b;

                     if (gridArray[oldGrid.x, oldGrid.y] != DataType.Player)
                         gridArray[oldGrid.x, oldGrid.y] = DataType.Water;
                 }
                 break;
             case DataType.Train:
                 Vector2Int trainHead = newGrid;
                 Vector2Int trainB1 = new Vector2Int(dir * -1, 0) + newGrid;
                 Vector2Int trainB2 = new Vector2Int(dir * -2, 0) + newGrid;
                 Vector2Int oldTGrid = new Vector2Int(dir * -3, 0) + newGrid;
                 if (GridInArray(trainHead.x))
                 {
                     if (gridArray[trainHead.x, trainHead.y] != DataType.Player)
                         gridArray[trainHead.x, trainHead.y] = DataType.Train;
                     if (gridArray[trainHead.x, trainHead.y] == DataType.Player)
                     {
                         //Debug.LogError("$Crash by Train");
                     }
                 }
                 if (GridInArray(trainB1.x))
                 {
                     if (gridArray[trainB1.x, trainB1.y] != DataType.Player)
                         gridArray[trainB1.x, trainB1.y] = DataType.Train;
                 }
                 if (GridInArray(trainB2.x))
                 {
                     if (gridArray[trainB2.x, trainB2.y] != DataType.Player)
                         gridArray[trainB2.x, trainB2.y] = DataType.Train;
                 }
                 if (GridInArray(oldTGrid.x))
                 {
                     if (gridArray[oldTGrid.x, oldTGrid.y] != DataType.Player)
                         gridArray[oldTGrid.x, oldTGrid.y] = DataType.None;
                 }
                 break;
         }
     }*/

    public void ObjectSizeChangeData(int dir, Vector2Int newGrid, DataType dataType)
    {
        switch (dataType)
        {
            case DataType.Crocodile_h:
                Vector2Int h = newGrid;
                Vector2Int b = new Vector2Int(-dir * 1, 0) + newGrid;
                Vector2Int oldGrid = new Vector2Int(-dir * 2, 0) + newGrid;
                if (GridInArray(h.x))
                {
                    Vector2Int hToG = GridToArray(h);
                    //if (gridArray[hToG.x, hToG.y] != DataType.Player)
                        gridArray[hToG.x, hToG.y] = DataType.Crocodile_h;
                }
                if (GridInArray(b.x))
                {
                    Vector2Int bToG = GridToArray(b);
                    //if (gridArray[bToG.x, bToG.y] != DataType.Player)
                        gridArray[bToG.x, bToG.y] = DataType.Crocodile_b;
                }
                if (GridInArray(oldGrid.x))
                {
                    Vector2Int oldGridToG = GridToArray(oldGrid);
                    //if (gridArray[oldGridToG.x, oldGridToG.y] != DataType.Player)
                        gridArray[oldGridToG.x, oldGridToG.y] = DataType.Water;
                }
                break;
            case DataType.Train:
                Vector2Int trainHead = newGrid;
                Vector2Int trainB1 = new Vector2Int(-dir * 1, 0) + newGrid;
                Vector2Int trainB2 = new Vector2Int(-dir * 2, 0) + newGrid;
                Vector2Int oldTGrid = new Vector2Int(-dir * 3, 0) + newGrid;

                if (GridInArray(trainHead.x))
                {
                    Vector2Int trainHeadToG = GridToArray(trainHead);
                    if (gridArray[trainHeadToG.x, trainHeadToG.y] != DataType.Player)
                        gridArray[trainHeadToG.x, trainHeadToG.y] = DataType.Train;
                    if (gridArray[trainHeadToG.x, trainHeadToG.y] == DataType.Player)
                    {
                        //Debug.LogError("$Crash by Train");
                        PlayerMenager.instance.ShowAniamtionDead(1);
                    }
                }
                if (GridInArray(trainB1.x))
                {
                    Vector2Int trainB1ToG = GridToArray(trainB1);
                    if (gridArray[trainB1ToG.x, trainB1ToG.y] != DataType.Player)
                        gridArray[trainB1ToG.x, trainB1ToG.y] = DataType.Train;
                }
                if (GridInArray(trainB2.x))
                {
                    Vector2Int trainB2ToG = GridToArray(trainB2);
                    if (gridArray[trainB2ToG.x, trainB2ToG.y] != DataType.Player)
                        gridArray[trainB2ToG.x, trainB2ToG.y] = DataType.Train;
                }
                if (GridInArray(oldTGrid.x))
                {
                    Vector2Int oldTGridToGrid = GridToArray(oldTGrid);
                    if (gridArray[oldTGridToGrid.x, oldTGridToGrid.y] != DataType.Player)
                        gridArray[oldTGridToGrid.x, oldTGridToGrid.y] = DataType.None;
                }
                break;
        }
    }
    void FindLogInGrid(Vector2Int targetGrid)
    {
        for (int i = 0; i < logDatas.Count; i++)
        {
            int objectGridY = GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y).y;
            //print("target is1 = " +GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y));
            if (objectGridY < playerGrid.y - 2 - maxMinusY)
            {
                continue;
            }
            if (objectGridY == playerGrid.y - 1 - maxMinusY || objectGridY == playerGrid.y - maxMinusY || objectGridY == playerGrid.y + 1 - maxMinusY) // +1 as player
            {
                Vector2Int newV2 = GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y);

                if (GridToArray(newV2) == targetGrid)
                {
                    playerTransform = logDatas[i].playerPoint.GetComponent<Transform>(); // And set move to pos
                    onLog = true;
                    // Set GridArray To Water
                    //player.transform.SetParent(logDatas[i].transform); // Change to set speed or transform *Problem is if out of array <= slove use GridCal.x if > 4 || < -4 DIed
                }
            }
        }
        // targetGrid = grid in database (5,12)
        //print($"target is =" + targetGrid);
    }
    void FindBodyInGrid(Vector2Int targetGrid)
    {
        for (int i = 0; i < gatorDatas.Count; i++)
        {
            int objectGridY = GridManager.Instance.VectorToGrid(gatorDatas[i].transform.position.x, gatorDatas[i].transform.position.y).y;
            //print("target is1 = " +GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y));
            if (objectGridY < playerGrid.y - 2 - maxMinusY)
            {
                continue;
            }
            if (objectGridY == playerGrid.y - 1 - maxMinusY || objectGridY == playerGrid.y - maxMinusY || objectGridY == playerGrid.y + 1 - maxMinusY) // +1 as player
            {
                Vector2Int newV2 = GridManager.Instance.VectorToGrid(gatorDatas[i].transform.position.x, gatorDatas[i].transform.position.y);
                Vector2Int newBodyPos = new Vector2Int(-gatorDatas[i].dis, 0) + newV2;
                Debug.Log("<COLOR=CYAN> new Ve2 = !</COLOR>" + newV2);
                Debug.Log("<COLOR=CYAN> new Ve2 to Array = !</COLOR>" + GridToArray(newV2));
                Debug.Log("<COLOR=CYAN> targetIs = !</COLOR>" + targetGrid);
                if (GridToArray(newBodyPos) == targetGrid)
                {
                    print("Gator 2");
                    playerTransform = gatorDatas[i].playerPoint.GetComponent<Transform>(); // And set move to pos
                    onLog = true;
                    // Set GridArray To Water
                    //player.transform.SetParent(logDatas[i].transform); // Change to set speed or transform *Problem is if out of array <= slove use GridCal.x if > 4 || < -4 DIed
                }
            }
        }
    }
    void FindInGrid(Vector2Int targetGrid)
    {
        for (int i = 0; i < gatorDatas.Count; i++)
        {
            int objectGridY = GridManager.Instance.VectorToGrid(gatorDatas[i].transform.position.x, gatorDatas[i].transform.position.y).y;
            //print("target is1 = " +GridManager.Instance.VectorToGrid(logDatas[i].transform.position.x, logDatas[i].transform.position.y));
            if (objectGridY < playerGrid.y - 2 - maxMinusY)
            {
                continue;
            }
            if (objectGridY == playerGrid.y - 1 - maxMinusY || objectGridY == playerGrid.y - maxMinusY || objectGridY == playerGrid.y + 1 - maxMinusY) // +1 as player
            {
                Vector2Int newV2 = GridManager.Instance.VectorToGrid(gatorDatas[i].transform.position.x, gatorDatas[i].transform.position.y);
                Vector2Int newBodyPos = new Vector2Int(-gatorDatas[i].dis, 0) + newV2;
                Vector2Int newBodyPos2 = new Vector2Int(gatorDatas[i].dis, 0) + newV2;
                Debug.Log("<COLOR=CYAN> new Ve2 = !</COLOR>" + newV2);
                Debug.Log("<COLOR=CYAN> new Ve2 to Array = !</COLOR>" + GridToArray(newV2));
                Debug.Log("<COLOR=CYAN> targetIs = !</COLOR>" + targetGrid);
                if (GridToArray(newV2) == targetGrid || GridToArray(newBodyPos) == targetGrid || GridToArray(newBodyPos2) == targetGrid)
                {
                    print("Gator 2");
                    playerTransform = gatorDatas[i].playerPoint.GetComponent<Transform>(); // And set move to pos
                    onLog = true;
                    // Set GridArray To Water
                    //player.transform.SetParent(logDatas[i].transform); // Change to set speed or transform *Problem is if out of array <= slove use GridCal.x if > 4 || < -4 DIed
                }
            }
        }
    }

    // Convert grid x < 0 && y < 0 To > 0
    public Vector2Int GridToArray(Vector2Int recivedValue)
    {
        int x = recivedValue.x + Mathf.FloorToInt(playZoneX / 2);
        int y = recivedValue.y + maxMinusY;
        return new Vector2Int(x, y);
    }
    bool GridInArray(float x)
    {
        if (x < -Mathf.FloorToInt(playZoneX / 2) || x > Mathf.FloorToInt(playZoneX / 2))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    int ConvertValue(int recivedValue)
    {
        return recivedValue % 30;
    }
    IEnumerator DelayCheck(Vector2Int targetGrid, DataType backupDataType)
    {
        yield return new WaitForSeconds(DelayCount);
        switch (gridArray[targetGrid.x, targetGrid.y]) // Change??
        {
            case DataType.Log:
                // Set To Parent
                // for loop find object in this grid and set parent || get speed and move with object
                SoundManager.Instance.PlaySound(SoundManager.Sound.jumpOnLog, audioSource);
                FindLogInGrid(targetGrid);
                // ChangeData With realTime
                break;
            case DataType.OB:
                Debug.LogError($"Crash by OB"); // Use Delay
                PlayerMenager.instance.ShowAniamtionDead(1);
                break;
            case DataType.Crocodile_b:
                FindBodyInGrid(targetGrid);
                GatorRive = false;
                break;
            case DataType.Water:
                Debug.LogError($"Drown"); // Use Delay
                onLog = false;
                playerTransform = null;
                PlayerMenager.instance.ShowAniamtionDead(2);
                break;
            case DataType.Crocodile_h:
                if (GatorRive)
                {
                    FindInGrid(targetGrid);
                    GatorRive = false;
                    break;
                }
                Debug.LogError($"Killed by gator"); // Use Delay
                PlayerMenager.instance.ShowAniamtionDead(2);
                onLog = false;
                playerTransform = null;
                break;
            case DataType.Train:
                Debug.LogError($"Crash the train"); // Use Delay
                PlayerMenager.instance.ShowAniamtionDead(1);
                break;
            default:
                onLog = false;
                playerTransform = null;
                break;
        }
        PlayerChangeData(playerGrid, targetGrid, backupDataType, DataType.Player);
        playerGrid = targetGrid;
    }
#if UNITY_EDITOR
    public Color RequestGridData(Vector2Int gridRequest)
    {
        DataType dataType = gridArray[gridRequest.x, gridRequest.y];
        switch (dataType)
        {
            case DataType.None:
                return Color.white;
            case DataType.Player:
                return Color.magenta;
            case DataType.Tree:
                return Color.yellow;
            case DataType.Log:
                return Color.white;
            case DataType.OB:
                return Color.red;
            case DataType.Crocodile_b:
                return new Color(1, 0.5f, 0);
            case DataType.Water:
                return Color.blue;
            case DataType.Crocodile_h:
                return Color.red;
            case DataType.Train:
                return Color.red;
            default:
                return Color.white;
        }
    }
#endif
}

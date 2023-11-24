using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using TMPro;
using System.Linq;

public class DirtLane : Lane
{
    public Animal[] AnimalPrefab;
    public Animal[] AnimalRevertDirectionPrefab;
    public Transform AnimalParent;
    private int startIndexPos = -4;
    private JSONNode animalPosition;
    private int moveDirection;
    private double moveSpeed;

    public TMP_Text text;

    private List<Animal> animals;
    private List<CarScript> animalsSorted;
    private List<GameObject> coins;
    [SerializeField] List<float> animalDelayTime = new List<float>();
    //---------------------------------------------------------------
    [Header("Edit Attribuild to Dirt Lane")]
    [SerializeField] float useTime;
    [SerializeField] Transform startPoint, endPoint;
    [SerializeField] List<Animal> AnimalList = new List<Animal>();
    private float rndSpeed;
    private int moveDirect;
    CarScript carScript;
    private int mirrorNum = 0;

    [Header("Setup coine in grass lane")]
    public GameObject Coine;
    public Transform CoineParent;
    private JSONArray CoinPosition;

    [Header("Back Ground")]
    [SerializeField]
    private SpriteRenderer dirtSprite;
    public Sprite[] backGroundSprite;


    [Header("Audio In Land")]
    [SerializeField]
    private AudioSource audioAnimal;
    [SerializeField]
    private AudioSource audioFootstep;

    [Header("Debug GridPivot")]
    [SerializeField]
    private GameObject gridPivot;

    public override void DeActive()
    {
        for(int i = 0; i < animals.Count; i++){
            DestroyImmediate(animals[i].gameObject);
        }

        for(int i = 0; i < coins.Count; i++){
            DestroyImmediate(coins[i].gameObject);
        }

        mirrorNum = 0;

        animals.Clear();
        coins.Clear();

        base.DeActive();
    }

    public override void Setup(JSONNode jsonData, int Lindex)
    {
        base.Setup(jsonData, Lindex);

        for (int i = 0; i < Floors.Length; i++)
        {
            Floors[i].gameObject.SetActive(true);
            Floors[i].SetType(2);
#if UNITY_EDITOR
            var gridPivotSpawn = Instantiate(gridPivot, Floors[i].transform);
            GridDisplay gd = gridPivotSpawn.GetComponent<GridDisplay>();
            gd.myGridPoint = GridManager.Instance.VectorToGrid(Floors[i].transform.position.x,
            Floors[i].transform.position.y) + new Vector2Int(4, 4);
            gridPivotSpawn.transform.parent = null;
#endif
        }
        animalPosition = jsonData["AnimalPosition"].AsArray;
        moveDirection = jsonData["MoveDirection"].AsInt;
        CoinPosition = jsonData["Coin"].AsArray;

        moveSpeed = jsonData["MoveSpeed"].AsDouble;
        text.text = jsonData["PatternID"].ToString();

        animals = new List<Animal>();
        animalsSorted = new List<CarScript>();

        JSONObject animalSetting = new JSONObject();
        animalSetting["direction"] = moveDirection;
        animalSetting["speed"] = moveSpeed;
        rndSpeed = animalSetting["speed"];
        moveDirect = animalSetting["direction"];

        //Animal prefab direction
        var prefab = AnimalPrefab;
        if(moveDirect == -1 && AnimalRevertDirectionPrefab != null)
        {
            prefab = AnimalRevertDirectionPrefab;
        }

        //Setup Animal
        animalSetting["size"] = 0;
        int animalInex = UnityEngine.Random.Range(0,AnimalPrefab.Length);
        int timeSound = UnityEngine.Random.Range(3,5);
        int numOfBg = UnityEngine.Random.Range(0,backGroundSprite.Length);

        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < animalPosition.Count; i++)
            {
                Vector3 mirrorPos = new Vector3(
                        GridManager.Instance.GridToVector(((j - 1) * 9), 0).x,
                        GridManager.Instance.GridToVector(((j - 1) * 9), 0).y);

                Animal animal = Instantiate<Animal>(prefab[animalInex], Vector3.zero, Quaternion.identity);
                animal.transform.parent = AnimalParent;
                animal.transform.localPosition = new Vector3(
                     x: GridManager.Instance.GridToVector(animalPosition[i] - 4, 0).x,
                     y: GridManager.Instance.GridToVector(animalPosition[i] - 4, 0).y,
                     z: 0) + mirrorPos;
                animal.Setup(animalSetting);
                animals.Add(animal);
                TerrainDirtLane(animal);

                carScript.carPos = (animalPosition[i] + (j - 1) * 9);
                carScript.carDelay = animalDelayTime[mirrorNum];
                carScript.useTime = useTime;
                carScript.gameObject.name = $"car [{carScript.carPos}]";
                mirrorNum++;
            }
        }
        foreach (var item in animals)
        {
            animalsSorted.Add(item.GetComponent<CarScript>());
            if (moveDirection == 1)
                animalsSorted = animalsSorted.OrderBy(w => -w.carPos).ToList();
            else
                animalsSorted = animalsSorted.OrderBy(w => w.carPos).ToList();
            item.GetComponent<CarScript>().obInLane = animals.Count;
        }
        GridDatabase.Instance.carDatas.AddRange(animalsSorted);

        SetupCoinInLand();

        gameObject.SetActive(true);
        dirtSprite.sprite = backGroundSprite[numOfBg];
        StartCoroutine(PalySoungInLand(animalInex,timeSound));
    }

    public void SetupCoinInLand()
    {
        coins = new List<GameObject>();
        for (int i = 0; i < CoinPosition.Count; i++)
        {
            GameObject coines = Instantiate<GameObject>(Coine);
            coines.transform.parent = CoineParent;
            coines.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(CoinPosition[i] + startIndexPos, 0).x,
                                                         GridManager.Instance.GridToVector(CoinPosition[i] + startIndexPos, 0).y, 0);
            coins.Add(coines);
        }
    }
    public override void StartActive()
    {
        base.StartActive();

        for(int i = 0; i < animals.Count; i++){
            animals[i].StartActive();
        }
    }
    public override void StopActive()
    {
        base.StopActive();

        for (int i = 0; i < animals.Count; i++)
        {
            animals[i].StopActive();
        }
    }
    public void randomPrefabsAnimals()
    {
        #region random
        //random animal
        for (int i = 0; i < AnimalPrefab.Length; i++)
        {
            i = UnityEngine.Random.Range(0, AnimalPrefab.Length);
            AnimalList.Add(AnimalPrefab[i]);
        }
        /*
        if (moveDirect == 1)
        {
            AnimalPrefab = animalDirSprite[0];
        }
        else if (moveDirect == -1)
        {
            AnimalPrefab = animalDirSprite[1];
        }*/
        #endregion  
    }

    public void TerrainDirtLane(Animal animal)
    {
        switch (moveDirect)
        {
            case -1:
                CalculateUseTime(animal, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                animalDelayTime.Add(Vector2.Distance(animal.transform.localPosition, startPoint.localPosition) / rndSpeed);
                //=======================================
                break;
            case 0:
                CalculateUseTime(animal, Vector2.Distance(endPoint.localPosition, startPoint.localPosition)); // Change position => local
                //================= new ================= R
                animalDelayTime.Add(Vector2.Distance(animal.transform.localPosition, startPoint.localPosition) / rndSpeed);
                break;
            case 1:
                CalculateUseTime(animal, Vector2.Distance(startPoint.localPosition, endPoint.localPosition)); // Change position => local
                //================= new ================= R
                animalDelayTime.Add(Vector2.Distance(endPoint.localPosition, animal.transform.localPosition) / rndSpeed);
                //=======================================
                break;
        }
    }

    public void CalculateUseTime(Animal animal, float dist)
    {
        useTime = dist / rndSpeed;
        carScript = animal.GetComponent<CarScript>();
        carScript.startPoint = startPoint;
        carScript.endPoint = endPoint;
        carScript.speed = rndSpeed;
        carScript.dir = moveDirect;
    }

    IEnumerator PalySoungInLand(int numOfAnimal,int timeSound)
    {
        while (true)
        {
            yield return new WaitForSeconds(timeSound);
            switch (numOfAnimal)
            {
                case 0:
                    SoundManager.Instance.PlaySound(SoundManager.Sound.bull, audioAnimal);
                    SoundManager.Instance.PlaySound(SoundManager.Sound.bull_footstep, audioFootstep);
                    break;
                case 1:
                    SoundManager.Instance.PlaySound(SoundManager.Sound.deer, audioAnimal);
                    SoundManager.Instance.PlaySound(SoundManager.Sound.deer_footstep, audioFootstep);
                    break;
                case 2:
                    SoundManager.Instance.PlaySound(SoundManager.Sound.jeep_horn, audioAnimal);
                    SoundManager.Instance.PlaySound(SoundManager.Sound.jeep, audioFootstep);
                    break;
                case 3:
                    SoundManager.Instance.PlaySound(SoundManager.Sound.spinningRock, audioAnimal);
                    SoundManager.Instance.PlaySound(SoundManager.Sound.spinningRock_footstep, audioFootstep);
                    break;
                default:
                    break;
            }
        }
    }

#if UNITY_EDITOR
    private bool enableDebug;

    void OnDrawGizmos() {
        if (!enableDebug) return;

        var c = Gizmos.color;
        Gizmos.color = Color.red;

        for (int x = -9; x <= 17; x++)
        {
            var p = GridManager.Instance.GridToVector(x-4, 0);
            Gizmos.DrawWireSphere(transform.TransformPoint(p), 0.2f);
        }

        Gizmos.color = c;

        // var p1 = GridManager.Instance.GridToVector(-9-4, 0);
        // var p2 = GridManager.Instance.GridToVector(-8-4, 0);
        // var p3 = GridManager.Instance.GridToVector(17-4, 0);
        // Debug.Log($"block size: {Vector3.Distance(transform.TransformPoint(p1), transform.TransformPoint(p2))}" );
        // Debug.Log($"left: {transform.TransformPoint(p1)}" );
        // Debug.Log($"right: {transform.TransformPoint(p1)}" );
    }

#endif
}

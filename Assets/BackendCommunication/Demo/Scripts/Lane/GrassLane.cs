using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using TMPro;

public class GrassLane : Lane
{
    public GameObject[] Trees;
    public GameObject[] LRTrees;
    public Sprite[] allTreeSprite;
    private List<GameObject> coins;
    private JSONArray treePosition;

    [Header("Setup coine in grass lane")]
    public GameObject Coine;
    public Transform CoineParent;
    private JSONArray CoinPosition;


    public TMP_Text text;

    [Header("Back Ground")]
    public Sprite[] backGroundSprite;
    [SerializeField]
    private SpriteRenderer grassSprite;

    [Header("Debug GridPivot")]
    [SerializeField]
    private GameObject gridPivot;
    public override void DeActive()
    {
        for (int i = 0; i < Trees.Length; i++){
            Trees[i].SetActive(false);
        }

        for(int i = 0; i < coins.Count; i++){
            DestroyImmediate(coins[i].gameObject);
        }

        coins.Clear();

        base.DeActive();
    }

    public override void Setup(JSONNode jsonData, int lIndex)
    {
        base.Setup(jsonData, lIndex);

        treePosition = jsonData["TreePosition"].AsArray;
        CoinPosition = jsonData["Coin"].AsArray;


        text.text = jsonData["PatternID"].ToString();
        for (int i = 0; i < Trees.Length; i++){
            Trees[i].SetActive(false);
#if UNITY_EDITOR
            var gridPivotSpawn = Instantiate(gridPivot, Trees[i].transform);
            GridDisplay gd = gridPivotSpawn.GetComponent<GridDisplay>();
            gd.myGridPoint = GridManager.Instance.VectorToGrid(Trees[i].transform.position.x,
            Trees[i].transform.position.y) + new Vector2Int(4, 4);
            gridPivotSpawn.transform.parent = null;
#endif
        }
        for (int i = 0; i < treePosition.Count; i++){
            Trees[treePosition[i]].SetActive(true);
            GridDatabase.Instance.SetTree(GridManager.Instance.VectorToGrid(Trees[treePosition[i]].transform.position.x,
            Trees[treePosition[i]].transform.position.y));
        }

        //Setup Coine
        coins = new List<GameObject>();
        for (int i = 0; i < CoinPosition.Count; i++)
        {
            GameObject coines = Instantiate<GameObject>(Coine);
            coines.transform.parent = CoineParent;
            coines.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).x,
            GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).y, 0);

            Debug.Log(new Vector3(GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).x,
            GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).y, 0));

            coins.Add(coines);
        }
        int numOfBg = UnityEngine.Random.Range(0, backGroundSprite.Length);
        grassSprite.sprite = backGroundSprite[numOfBg];
        foreach (var item in LRTrees)
        {
            int rnd = UnityEngine.Random.Range(0,allTreeSprite.Length);
            item.GetComponent<SpriteRenderer>().sprite = allTreeSprite[rnd];
        }
        gameObject.SetActive(true);
    }

    public override void StartActive()
    {
        base.StartActive();
    }
}

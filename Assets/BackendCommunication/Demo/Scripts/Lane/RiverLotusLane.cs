using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using TMPro;

public class RiverLotusLane : Lane
{
    public GameObject[] Lotus;
    private List<GameObject> coins;
    private JSONArray lotusPosition;
    public TMP_Text text;

    [Header("Set up Coin in lane")]
    public GameObject Coine;
    public Transform CoineParent;
    private JSONArray CoinPosition;

    [Header("Debug GridPivot")]
    [SerializeField]
    private GameObject gridPivot;
    public override void DeActive()
    {
        for(int i = 0; i < Lotus.Length; i++){
            Lotus[i].SetActive(false);
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

        lotusPosition = jsonData["LotusPosition"].AsArray;
        CoinPosition = jsonData["Coin"].AsArray;

        Debug.Log(lotusPosition.ToString());

        text.text = jsonData["PatternID"].ToString();
        for (int i = 0; i < Lotus.Length; i++)
        {
            Lotus[i].SetActive(false);
            GridDatabase.Instance.AddObjectToArray(
                GridManager.Instance.VectorToGrid(Lotus[i].transform.position.x,
                Lotus[i].transform.position.y), 6);
#if UNITY_EDITOR
            var gridPivotSpawn = Instantiate(gridPivot, Lotus[i].transform);
            GridDisplay gd = gridPivotSpawn.GetComponent<GridDisplay>();
            gd.myGridPoint = GridManager.Instance.VectorToGrid(Lotus[i].transform.position.x,
            Lotus[i].transform.position.y) + new Vector2Int(4, 4);
            gridPivotSpawn.transform.parent = null;
#endif
        }
        // TODO วางใบบัวตามตำแหน่ง lotusPosition
        for (int i = 0; i < lotusPosition.Count; i++){
            Lotus[lotusPosition[i]].SetActive(true);
            GridDatabase.Instance.AddObjectToArray(new Vector2Int(lotusPosition[i] - 4, lIndex - 4), 0);

            Debug.Log("Lotus Position" + new Vector2Int(lotusPosition[i] - 4, lIndex - 4));

        }
        //Setup Coine
        coins = new List<GameObject>();
        for (int i = 0; i < CoinPosition.Count; i++)
        {
            GameObject coines = Instantiate<GameObject>(Coine);
            coines.transform.parent = CoineParent;
            coines.transform.localPosition = new Vector3(GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).x,
                                                         GridManager.Instance.GridToVector(CoinPosition[i] - 4, 0).y, 0);
            coins.Add(coines);
        }

        gameObject.SetActive(true);
    }

    public override void StartActive()
    {
        base.StartActive();
    }
}

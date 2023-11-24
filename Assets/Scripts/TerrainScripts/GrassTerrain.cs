using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTerrain : MonoBehaviour
{
    [SerializeField] GameObject tree;
    int treeCount = 0;
    [SerializeField] List<GameObject> treeList = new List<GameObject>();

    public void SpawnTree(int count)
    {
        treeCount = count;
        for (int i = 0; i < treeCount; i++)
        {
            var treeSpawn = Instantiate(tree, gameObject.transform); //Optimize : Spawn on start and use on&off + Change transform
            int rndPosition = Random.Range((int)-Database.Terrain.playZone / 2, (int)Database.Terrain.playZone / 2);
            treeSpawn.transform.localPosition = GridManager.Instance.GridToVector(rndPosition , 0);
            treeList.Add(treeSpawn);
        }
    }
    private void OnEnable()
    {
        SpawnTree(3);
    }
    private void OnDisable()
    {
        foreach (var item in treeList)
        {
            Destroy(item); // Change to SetActive(false);
        }
        treeList.Clear();
    }
}

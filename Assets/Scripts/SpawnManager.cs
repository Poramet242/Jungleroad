using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    enum MocUpTerrain
    {
        Grass,
        Road,
        Water,
        Large
    }
    readonly Queue<GameObject> grassQueue = new Queue<GameObject>();
    readonly Queue<GameObject> dirtQueue = new Queue<GameObject>();
    readonly Queue<GameObject> waterQueue = new Queue<GameObject>();
    readonly Queue<GameObject> largeQueue = new Queue<GameObject>();
    readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    //------------- On Editor -------------//
    [SerializeField] GameObject[] terrain;
    [SerializeField] MocUpTerrain mocUpTerrain;
    [SerializeField] Grid grid;
    [SerializeField] Vector2 lastSpawnPos;

    void Start()
    {
        //print(Database.Terrain.preSpawn);
        for (int i = 0; i < System.Enum.GetValues(typeof(MocUpTerrain)).Length; i++)
        {
            SetUpTerrain(i);
        }
    }

    void SetUpTerrain(int type)
    {
        for (int i = 0; i < Database.Terrain.preSpawn; i++)
        {
            var terrainSpawn = Instantiate(terrain[type], grid.transform);
            terrainSpawn.SetActive(false);
            switch (mocUpTerrain)
            {
                case MocUpTerrain.Grass:
                    grassQueue.Enqueue(terrainSpawn);
                    break;
                case MocUpTerrain.Road:
                    dirtQueue.Enqueue(terrainSpawn);
                    break;
                case MocUpTerrain.Water:
                    waterQueue.Enqueue(terrainSpawn);
                    break;
                case MocUpTerrain.Large:
                    largeQueue.Enqueue(terrainSpawn);
                    break;
                default:
                    Debug.LogError("Out of Range");
                    break;
            }
        }
        switch (mocUpTerrain)
        {
            case MocUpTerrain.Grass:
                poolDictionary.Add(mocUpTerrain.ToString(), grassQueue);
                mocUpTerrain = MocUpTerrain.Road;
                break;
            case MocUpTerrain.Road:
                poolDictionary.Add(mocUpTerrain.ToString(), dirtQueue);
                mocUpTerrain = MocUpTerrain.Water;
                break;
            case MocUpTerrain.Water:
                poolDictionary.Add(mocUpTerrain.ToString(), waterQueue);
                mocUpTerrain = MocUpTerrain.Large;
                break;
            case MocUpTerrain.Large:
                poolDictionary.Add(mocUpTerrain.ToString(), largeQueue);
                break;
            default:
                Debug.LogError("Out of Range");
                break;
        }

    }
    void SpawnTerrain()
    {
        GameObject terrain = poolDictionary[mocUpTerrain.ToString()].Dequeue();
        terrain.SetActive(true);
        // Set value to terrain
        Vector2 newSpawnPos = new Vector2(lastSpawnPos.x + (grid.cellSize.x / 2), lastSpawnPos.y + (grid.cellSize.y / 2));
        lastSpawnPos = newSpawnPos;
        terrain.transform.position = newSpawnPos;
        poolDictionary[mocUpTerrain.ToString()].Enqueue(terrain);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnTerrain();
        }
    }
}

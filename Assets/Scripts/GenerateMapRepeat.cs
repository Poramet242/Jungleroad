using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMapRepeat : MonoBehaviour
{
    public static GenerateMapRepeat Instance;
    private void Awake()
    {
        if (Instance != null)
            DestroyImmediate(gameObject);
            
        if (Instance == null)
            Instance = this;
    }

    [SerializeField]
    private MapController mapController;
    private int moveScore =  20;
    public void GenerateMap(int scoreMove)
    {
        if (scoreMove == moveScore) 
        {
            mapController.GenerateRepeat(20);
            mapController.DeSpawnLane(moveScore == 20? 15:20);
            Debug.Log($"Map Repeat" + scoreMove);
            moveScore += 20;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public Vector2 GridToVector(int front,int back)
    {
        float posX = (front + back) / 2f;
        float posY = (back - front) / 4f;
        return new Vector2(posX,posY);
    }

    public Vector2Int VectorToGrid(float x , float y)
    {
        //x = (gridX + gridY) /2
        //y = (gridY - gridX)
        int gridX = Mathf.RoundToInt((x/0.5f + y/-0.25f) * 0.5f);
        int gridY = Mathf.FloorToInt(-(y/-0.25f - x/0.5f) * 0.5f);
        return new Vector2Int(gridX,gridY);
    }
}

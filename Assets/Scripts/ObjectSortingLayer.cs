using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSortingLayer : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public int startLayer, layer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(ScoreManager.Instance.playerLastY == startLayer)
        {
            spriteRenderer.sortingOrder = layer;
        }
    }
}

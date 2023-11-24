using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class TreeBlock : Block
{
    public override void Setup(JSONNode data)
    {
        BlockCollider.isTrigger = false;
    }

    public override void StartActive()
    {
        throw new System.NotImplementedException();
    }

    public override void StopActive()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

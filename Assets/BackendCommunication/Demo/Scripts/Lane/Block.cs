using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public abstract class Block : MonoBehaviour
{
    public enum BlockTypeEnum {
        Tree = 1,
        Animal = 5,
        Log = 3
    }

    BlockTypeEnum blockType;

    public Collider2D BlockCollider;

    public abstract void Setup(JSONNode data);

    public abstract void StartActive();

    public abstract void StopActive();
}

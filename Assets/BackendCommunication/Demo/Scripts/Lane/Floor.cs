using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public enum FloorTypeEnum {
        Grass = 1,
        Dirt = 2,
        River = 3,
        Lotus = 4,
    }

    public Renderer Renderer;

    public Material GrassMaterial;
    public Material DirtMaterial;
    public Material RiverMaterial;
    public Material LotusMaterial;

    FloorTypeEnum FloorType;

    public void SetType(int typeInt){
        FloorType = (FloorTypeEnum)typeInt;
        switch(FloorType){
            case FloorTypeEnum.Grass:
            Renderer.material = GrassMaterial;
            break;
            case FloorTypeEnum.Dirt:
            Renderer.material = DirtMaterial;
            break;
            case FloorTypeEnum.River:
            Renderer.material = RiverMaterial;
            break;
            case FloorTypeEnum.Lotus:
            Renderer.material = LotusMaterial;
            break;
        }
    }
}

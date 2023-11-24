using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListEffectsManager : MonoBehaviour
{
    public static ListEffectsManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    [Header("Effects List")]
    public List<GameObject> effect = new List<GameObject>();
}

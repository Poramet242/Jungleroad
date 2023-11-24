using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTransferScene : MonoBehaviour
{
    public static DataTransferScene Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }
    public RuntimeAnimatorController[] animatorController;
    public int selectNumber;
}

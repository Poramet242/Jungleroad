using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    // nextSeed is an temporary varible which store the seed given by server when user connected via websocket
    // TODO: chanyutl - find out the better way to handle storing seed and pass it along the scenes
    public SetAndGetOnce<uint> nextSeed;

    public static DataManager Instance;

    //public Database.Terrain database = new Database.Terrain();
    private void Awake()
    {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
        /*SceneManager.sceneLoaded += (scene, mode) => {
            var obj = GameObject.Find("GameOverBGPanel");
            if (obj == null) {
                Debug.Log("[SceneLoaded] cannot find GameOverBGPanel");
            }
            gameoverPanel = obj;
        };*/

        DontDestroyOnLoad(this.gameObject);
    }

    
    public DateTime gameStartTime;
}


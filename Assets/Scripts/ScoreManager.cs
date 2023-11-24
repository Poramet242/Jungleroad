using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public int score = 0;
    public int coin = 0;
    public int playerLastY;
    public GameObject gameoverPanel;
    public GameObject CheckPointPrefab;
    public Transform CheckPointParant;
    private string MilestoneText;
    private int MilestoneScore;
    public void ScorePlus(int lastY)
    {
        if (lastY > playerLastY)
        {
            playerLastY = lastY;
            score++;
        }
    }
    public void coinesPlus()
    {
        coin++;
    }
    public void CheckPoint(int valueMapData)
    {
        MilestoneScore = valueMapData;
        GameObject Milestone = Instantiate<GameObject>(CheckPointPrefab);
        Milestone.transform.SetParent(CheckPointParant);
        MilestoneText = "Milestone " + MilestoneScore + " score";
        Milestone.GetComponentInChildren<TMP_Text>().text = MilestoneText;
        Milestone.transform.localPosition = GridManager.Instance.GridToVector(0, MilestoneScore + 1);
    }
}

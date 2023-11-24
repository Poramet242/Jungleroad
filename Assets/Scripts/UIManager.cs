using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Backend;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    public ScenFader fader;
    public Text scoreText;
    public Text timeText;
    public Text CoinJText;
    [Header("Player Dead")]
    public PlayerMenager playermanager;
    public GameObject ButtonEndGame;
    private float timeSpened;
    private bool CheckTime = false;

    public bool CheckEndGame;
    public bool CheckErrorGame;

    [Header("Score point endgame")]
    public Text ScoreCoineJGROText;
    public Text ScoreDistanceText;
    public Text ScoreCoinJText;
    public Text ScoreTimeText;
    [Header("Rank Images")]
    public Image PosRankImages;
    public Sprite[] RankImagesArr;
    [SerializeField]
    AudioSource audioSource;

    [Header("Error Message")]
    public GameObject ErrorMessageObj;


    private void Start()
    {
        CheckTime = false;
    }
    void Update()
    {
        if (!CheckTime)
        {
            timeSpened += Time.deltaTime;
            scoreText.text = ScoreManager.Instance.score.ToString();
            CoinJText.text = ScoreManager.Instance.coin.ToString();
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeSpened);
            timeText.text = timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");
        }
        // if (CheckErrorGame&&!CheckEndGame)
        // {
        //     ErrorMessageObj.SetActive(true);
        // }
    }

    public void ShowErrorMessageObject(string closeCode){
        if (!ConnectToServer.getInstance().IsGameEnding())
        {
            Debug.Log($"Show Connection Close Error with CloseCode:{closeCode} while gamestate: {ConnectToServer.getInstance().GetGameState()}");
            ErrorMessageObj.SetActive(true);
        }
    }

    // public void ShowError()
    // {
    //     if (API.isAlphaTest && CheckEndGame)
    //     {
    //         return;
    //     }
    //     ErrorMessageObj.SetActive(true);
    // }

    public void ResultTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeSpened);
        ScoreTimeText.text = timeSpan.Hours.ToString("00") + ":" + timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");
        if (API.isAlphaTest)
        {
            ScoreCoinJText.text = CoinJText.text;
            ScoreDistanceText.text = scoreText.text;
            PosRankImages.gameObject.SetActive(true);
            PosRankImages.sprite = RankImagesArr[3];
        }
        CheckTime = true;
    }
    //Calculate Score to server
    public void ShowCalculateResult(GameEndResultPackage result)
    {
        ScoreCoineJGROText.text = result.TotalReward.ToString();
        ScoreCoinJText.text = result.CoinCollected.ToString();
        //Case Distance
        if (result.Distance - 4 <= 0)
        {
            result.Distance = 0;
        }
        else
        {
            result.Distance = result.Distance - 4;
        }
        ScoreDistanceText.text = result.Distance.ToString();
        //Case Rank
        PosRankImages.gameObject.SetActive(true);
        switch (result.Rank)
        {
            case "A":
                PosRankImages.sprite = RankImagesArr[0];
                break;
            case "B":
                PosRankImages.sprite = RankImagesArr[1];
                break;
            case "C":
                PosRankImages.sprite = RankImagesArr[2];
                break;
            case "D":
                PosRankImages.sprite = RankImagesArr[3];
                break;
            case "E":
                PosRankImages.sprite = RankImagesArr[4];
                break;
            case "F":
                PosRankImages.sprite = RankImagesArr[5];
                break;
            case "S":
                PosRankImages.sprite = RankImagesArr[6];
                break;
            default:
                PosRankImages.sprite = RankImagesArr[0];
                break;
        }
        ButtonEndGame.SetActive(true);
    }

    public void CallDead()
    {
        //Debug.Log("CallDead !!");
        PlayerMenager.instance.enabled = false;
        CheckEndGame = true;
        StartCoroutine(EndGame());
    }
    public void RestartGame()
    {
        fader.FadeTo("Mainmenu_P");
    }
    public void onClickButtonEff()
    {
        SoundManager.Instance.PlaySound(SoundManager.Sound.button, audioSource);
    }
    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(1.75f);
        SoundManager.Instance.PlaySound(SoundManager.Sound.show_ui, audioSource);
        ScoreManager.Instance.gameoverPanel.SetActive(true);
        if (API.isAlphaTest)
        {
            ButtonEndGame.SetActive(true);
        }
        ResultTime();
    }
}

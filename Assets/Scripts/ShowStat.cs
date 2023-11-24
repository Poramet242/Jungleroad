using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowStat : MonoBehaviour
{
    public bool showStat;

    public Text fpsTxt, pingTxt, avgPingTxt;

    public float updateInterval = 0.5f;
    float accum = 0.0f;
    int frames = 0;
    float timeleft;
    float fps;
    [SerializeField]
    GameWorld gameWorld;

    // Start is called before the first frame update
    void Start()
    {
        if(!showStat)
        {
            fpsTxt.gameObject.SetActive(false);
            pingTxt.gameObject.SetActive(false);
            avgPingTxt.gameObject.SetActive(false);
            this.enabled = false;
        }
        timeleft = updateInterval;
    }

    // Update is called once per frame
    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            fps = (accum / frames);
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
            fpsTxt.text = "FPS : " + fps.ToString("F2");
        }
        pingTxt.text = "Ping : " + gameWorld.GetPing().ToString("F2");
        avgPingTxt.text = "Avg : "+ gameWorld.GetAvgPing().ToString("F2");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainTerrain : MonoBehaviour
{
    public Transform startPoint, endPoint;
    public GameObject trainPrefab;
    
    private int delay;
    // Start is called before the first frame update
    void Start()
    {
        delay = Random.Range(5, 10);
        InvokeRepeating(nameof(LoopWarning), 0, delay);
        var trainSpawn = Instantiate(trainPrefab, gameObject.transform);
        TrainScript trainScript = trainSpawn.GetComponent<TrainScript>();
        trainScript.delay = delay;
        trainScript.startPoint = startPoint;
        trainScript.endPoint = endPoint;
    }
    private void OnDisable()
    {
        CancelInvoke();
    }
    void LoopWarning()
    {
        StartCoroutine(Warning(delay - 4));
    }

    [SerializeField] Sprite red,green;
    [SerializeField] GameObject trafficLight;
    [SerializeField] AudioSource audioSource;
    IEnumerator Warning(int warningTime)
    {
        SpriteRenderer lightSprite = trafficLight.GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(warningTime);
        lightSprite.sprite = red;
        yield return new WaitForSeconds(2f);
        SoundManager.Instance.PlaySound(SoundManager.Sound.traffic, audioSource);
        lightSprite.sprite = green;
        yield return new WaitForSeconds(2f);
        lightSprite.sprite = red;
    }

}

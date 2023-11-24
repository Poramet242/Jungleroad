using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainScript : MonoBehaviour, IGameWorldUpdatable
{
    public Transform startPoint, endPoint;
    public AnimationCurve moveCurve;
    public float delay;
    bool active;
    public float speed;
    float t;
    public Type type;
    public enum Type
    {
        snake,
        orcish,
        elphant
    }
    public int movDir;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 pos0;
    private Vector3 dir;
    
    [SerializeField] bool enabledDebug;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Delay());
        // transform.position = startPoint.position;
        // direction = (endPoint.position - startPoint.position).normalized;
        // InvokeRepeating(nameof(Active), delay, delay);

        int numGs = -9;
        int numGe = 18;
        var gs = GridManager.Instance.GridToVector(numGs - 4, 0);
        var ge = GridManager.Instance.GridToVector(numGe - 4, 0);
        
        startPos = transform.parent.TransformPoint(gs);
        endPos = transform.parent.TransformPoint(ge);

        if (movDir > 0)
        {
            transform.position = startPos;
        }
        else if (movDir < 0)
        {
            transform.position = endPos;
        }

        transform.position = startPos;
        pos0 = transform.position;

        dir = (endPoint.position - startPoint.position).normalized;
        if (movDir < 0) 
        {
            dir = -dir;
        }
        GridDatabase.Instance.trainDatas.Add(this);
        GameWorld.addMovebableObject(this);
    }

    void OnDestroy() {
        GameWorld.removeMovableObject(this);
    }

    Vector2 direction;

    // Update is called once per frame
    // void Update()
    // {
    //     if(active)
    //         gameObject.transform.Translate(direction * speed * Time.deltaTime);
    // }

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource audioSourceFootstep;
    private void Reset()
    {
        gameObject.transform.position = startPoint.position;
        active = false;
    }

    private void DelaySound()
    {
        switch (type)
        {
            case Type.snake:
                SoundManager.Instance.PlaySound(SoundManager.Sound.snake, audioSource);
                SoundManager.Instance.PlaySound(SoundManager.Sound.snake_footstep, audioSourceFootstep);
                break;
            case Type.orcish:
                SoundManager.Instance.PlaySound(SoundManager.Sound.ostrichs, audioSource);
                SoundManager.Instance.PlaySound(SoundManager.Sound.ostrichs__footstep, audioSourceFootstep);
                break;
            case Type.elphant:
                SoundManager.Instance.PlaySound(SoundManager.Sound.elephant, audioSource);
                SoundManager.Instance.PlaySound(SoundManager.Sound.elephant_footstep, audioSourceFootstep);
                break;
            default:
                break;
        }
    }
    IEnumerator Delay()
    {
        while (true) 
        {
            yield return new WaitForSeconds(delay);
            DelaySound();
            // InvokeRepeating(nameof(Reset), delay, delay);
        }
    }

    void Active()
    {
        active = true;
    }

    public void GameUpdate(float gameTime)
    {
        const string logTag = "[TrainScript::GameUpdate]";
        float D = Vector3.Distance(startPos, endPos);
        gameObject.transform.position = CalculateTrainHeadPosition(gameTime, delay, speed, 3, movDir, D);
    }

    // calculateTrainTravelingDistance calculate the traveling distance of train with reference start position at 0
    // the formula...
    // 		d(t) = ((t div T) * D) + (fts(t, td, T) * ((t mod T) - td) * v)    	--- (1)
    // 		T = td + (D / v) 	  												--- (2)
    // by given
    // 		+ d(t) -- traveling distance function
    // 		+ t -- time (เวลาที่ผ่านไป)
    // 		+ td -- train delay (เวลาที่หยุดรอในแต่ละรอบ)
    // 		+ v -- train speed (ความเร็ว)
    // 		+ D -- lap distance (ระยะทางต่อ 1 รอบวิ่ง)
    // 		+ T -- lap time (เวลารวมใน 1 รอบ) = td (เวลาที่หยุดรอ) + (D/v) (เวลาที่ใช้ คือ ระยะทาง หารด้วย ความเร็ว)
    // 		+ fts(t, td, T) -- time step function
    // 			fts = 0 if (t mod T) <= td
    // 			fts = 1 if (t mod T) > td
    float calculateTrainTravelingDistance(float t, float lapDistance, float trainDelay, float trainSpeed)
    {
        var td = trainDelay;
        var v = trainSpeed;
        var D = lapDistance;
        var T = td + (D / v);
        return (divF(t, T) * D) + (fts(t, td, T) * ((modF(t, T) - td) * v));
    }


    // CalculateTrainCurrentPosition ...
    // assume that train alway start moving at edge either left or right of the lane
    Vector3 CalculateTrainHeadPosition(float gameTime, float trainDelay, float trainSpeed, int trainSize, int travelDirection, float lapDistance)
    {
        const string logTag = "[TrainScript::CalculateTrainHeadPosition]";

        float laneLength = Vector3.Distance(this.startPos, this.endPos);
        float blockLength = laneLength / 27;
        float trainStartPos = .0f;
        if (travelDirection == 1)
        {
            // trainStartPos = (-9 + trainSize - 1) * blockLength;
            // trainStartPos = (trainSize - 1) * blockLength;
            trainStartPos = 0;
        }
        else if (travelDirection == -1)
        {
            // trainStartPos = (17 - trainSize + 1) * blockLength;
            // trainStartPos = (26 - trainSize + 1) * blockLength;
            trainStartPos = 27 * blockLength;
        }

        float distance = calculateTrainTravelingDistance(gameTime, lapDistance, trainDelay, trainSpeed);

        float d0 = 0;
        float d = 0;
        float r = 0;
        float remain = 0;

        var D = lapDistance;
        var end = D;
        Vector3 trainPos = Vector3.zero;
        if (travelDirection == 1)
        {
            d0 = trainStartPos;
            d = d0 + distance;
            r = Mathf.Floor(d / D);
            remain = d - (r * D);
            trainPos = this.startPos + (remain * dir);
        }
        else if (travelDirection == -1)
        {
            d0 = trainStartPos - end;
            if (d0 < 0)
            {
                d0 = -d0;
            }
            d = d0 + distance;
            r = Mathf.Floor(d / D);
            remain = d - (r * D);
            trainPos = this.endPos + (remain * dir);
        }

#if UNITY_EDITOR
        if (enabledDebug) {
            Debug.Log(string.Join("\n", 
            $"{logTag} pos0: {pos0}", 
            $"gameTime: {gameTime}",
            $"startPoint: {startPos}",
            $"endPoint: {endPos}",
            $"dir: {dir}",
            $"d0: {d0} d: {d} r: {r} remain: {remain}", 
            $"trainPos: {trainPos}",
            $"lapPos: {remain}"));
        }
#endif

        return trainPos;
    }

    #region Math helper functions

    float fts(float t_, float td_, float T_)
    {
        return (modF(t_, T_) <= td_) ? 0 : 1;
    }

    // divF floating point division only return the quotient
    float divF(float dividen, float divisor)
    {
        // prevent divide by zero panic
        if (divisor == 0)
        {
            return 0;
        }

        return Mathf.FloorToInt(dividen / divisor);
    }

    // modF floating point modulo only return the remainder of division
    float modF(float dividen, float divisor)
    {
        // prevent divide by zero panic
        if (divisor == 0)
        {
            return 0;
        }

        var quotient = Mathf.FloorToInt(dividen / divisor);
        return dividen - (quotient * divisor);
    }

    #endregion
#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (enabledDebug) {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(this.startPos, 0.1f * Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(this.endPos, 0.1f * Vector3.one);

            Gizmos.color = Color.magenta;
        }

        Gizmos.color = new Color(0, 0, 255f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0), transform.position + new Vector3(0, 0.5f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.25f, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0), transform.position);
        if (movDir > 0)
        {
            Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0), transform.position + new Vector3(0, 0.5f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.75f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.75f, 0), transform.position + new Vector3(-1f, 0.5f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-1f, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.25f, 0));

            Gizmos.DrawLine(transform.position + new Vector3(-1f, 0.5f, 0), transform.position + new Vector3(-0.5f, 0.75f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.75f, 0), transform.position + new Vector3(-1f, 1f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-1f, 1f, 0), transform.position + new Vector3(-1.5f, 0.75f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(-1.5f, 0.75f, 0), transform.position + new Vector3(-1f, 0.5f, 0));
        }
        else
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.5f, 0.25f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0.5f, 0.25f, 0), transform.position + new Vector3(1f, 0, 0));
            Gizmos.DrawLine(transform.position + new Vector3(1f, 0, 0), transform.position + new Vector3(0.5f, -0.25f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.25f, 0), transform.position);

            Gizmos.DrawLine(transform.position + new Vector3(0.5f, -0.25f, 0), transform.position + new Vector3(1f, 0f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(1f, 0f, 0), transform.position + new Vector3(1.5f, -0.25f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(1.5f, -0.25f, 0), transform.position + new Vector3(1f, -0.5f, 0));
            Gizmos.DrawLine(transform.position + new Vector3(1f, -0.5f, 0), transform.position + new Vector3(0.5f, -0.25f, 0));
        }
    }
#endif
}

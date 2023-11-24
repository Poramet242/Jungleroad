using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour
{
    [Header("Target Setting")]
    public Vector3 offset;
    public Transform target;
    public int colorIndex = 0;

    [Header("Trail")]
    private TrailRenderer trail;
    public List<Gradient> TrailColor;

    [Header("Particle")]
    private ParticleSystem ps;
    private ParticleSystem.ColorOverLifetimeModule col;
    public List<Gradient> particleColor;



    // Start is called before the first frame update
    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        trail.colorGradient = TrailColor[0];

        ps = GetComponentInChildren<ParticleSystem>();
        col = ps.colorOverLifetime;
        col.color = particleColor[0];
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        /*if(Input.GetMouseButtonDown(0))
        {
            colorIndex++;
            if(colorIndex >= TrailColor.Count)
            {
                colorIndex = 0;
            }
        }*/
        trail.colorGradient = TrailColor[colorIndex];
        col.color = particleColor[colorIndex];
    }
}

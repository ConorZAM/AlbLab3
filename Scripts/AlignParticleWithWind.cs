using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignParticleWithWind : MonoBehaviour
{
    ParticleSystem.MainModule ps;
    public GlobalWind globalWind;
    
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>().main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(new Vector3(0, 0, 1), globalWind.earthWindVector);
        ps.startSpeed = globalWind.earthWindVector.magnitude/5;
    }
}

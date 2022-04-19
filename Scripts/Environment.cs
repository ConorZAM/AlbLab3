using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    
    [Range(0, 30)]
    public float windSpeed=1;
    
    [Range(-180, 180)]
    public float windAzimuth=0;

    [Range(-180,180)]
    public float windElevation=0;



    //public Vector3 onTestRigBodyVelocity;


    public Vector3 earthWindVector;

    float airDensity = 1.2f;
    

    
    float deg2rad;

    public static string NewLine { get; internal set; }

    // public OceanAdvanced ocenAdvanced;
    // Start is called before the first frame update
    void Start()
    {
        deg2rad = Mathf.PI / 180;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //earthWindVector = windSpeed * new Vector3(Mathf.Sin(deg2rad * windDirection), 0, Mathf.Cos(deg2rad * windDirection));
        earthWindVector = Quaternion.Euler( windElevation, windAzimuth,0) * Vector3.forward * windSpeed;
    }
}

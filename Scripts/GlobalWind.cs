using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalWind : MonoBehaviour
{
    // -----------------------------------------------------------
    // -----------------------------------------------------------
    // MAKE SURE TO SET THE EXECUTION ORDER FOR THIS SCRIPT!!!!!
    // -----------------------------------------------------------
    // -----------------------------------------------------------

    static GlobalWind _singleton;
    public static GlobalWind Singleton()
    {
        if (_singleton == null)
        {
            _singleton = FindObjectOfType<GlobalWind>();
            _singleton.Initialise();
        }
        return _singleton;
    }

    [Range(0, 30)]
    public float windSpeed = 1;

    [Range(-180, 180)]
    public float windAzimuth = 0;

    [Range(-180, 180)]
    public float windElevation = 0;


    public Vector3 earthWindVector;

    // All the aero bodies found in the scene at Start - not searching for new ones during fixed update
    // as this would be a large overhead on the scene
    AeroBody[] aeroBodies;

    // Start is called before the first frame update
    void Awake()
    {
        // Get all the aero bodies in the scene. This is assuming that no new bodies are
        // created at runtime
        Initialise();

        // If we didn't find any then we don't need this component to be running
        if (aeroBodies.Length == 0)
        {
            this.enabled = false;
        }
    }

    public void SetWindVelocity()
    {
        earthWindVector = Quaternion.Euler(windElevation, windAzimuth, 0) * Vector3.forward * windSpeed;
        for (int i = 0; i < aeroBodies.Length; i++)
        {
            aeroBodies[i].externalFlowVelocity_inEarthFrame = earthWindVector;
        }
    }

    public void Initialise()
    {
        aeroBodies = FindObjectsOfType<AeroBody>();
        for (int i = 0; i < aeroBodies.Length; i++)
        {
            aeroBodies[i].Initialise();
        }
    }

    // The script execution order for this script must be a negative value
    // so that this is called before the aerodynamic models run their fixed updates
    void FixedUpdate()
    {
        SetWindVelocity();
    }
}
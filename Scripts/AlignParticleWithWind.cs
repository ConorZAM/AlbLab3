using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignParticleWithWind : MonoBehaviour
{
    ParticleSystem.ShapeModule psShape;
    ParticleSystem.MainModule psMain;
    public AeroBody aeroBody;
    public ThinAerofoilComponent liftingBody;
    
    [Tooltip("Use this to flip the vortex direction for left or right wing tips")]
    public float vortexFlip = 1f;

    bool useWindSpeed = true;
    
    // Start is called before the first frame update
    void Start()
    {
        psMain = GetComponent<ParticleSystem>().main;
        psShape = GetComponent<ParticleSystem>().shape;

        if(FlightDynamicsLabManager.Singleton().Settings.jointState == ExperimentSettings.JointState.Free)
        {
            ParticleSystem.EmissionModule emission = GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = 0;
            emission.rateOverDistance = 100;
            useWindSpeed = false;
        }
        else
        {
            ParticleSystem.EmissionModule emission = GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = 100;
            emission.rateOverDistance = 0;
            useWindSpeed = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!useWindSpeed || aeroBody.earthFrame.windVelocity.sqrMagnitude == 0f)
        {
            psMain.startSpeed = 0f;
        }
        else
        {
            transform.forward = -aeroBody.earthFrame.windVelocity.normalized;

            // Might not need this line
            psMain.startSpeed = aeroBody.earthFrame.windVelocity.magnitude / 10f;
        }
        

        // Voritcity babyyyyy - total guess on how it scales
        psShape.arcSpeed = vortexFlip * liftingBody.CL * 0.918246f;
        psShape.radius = Mathf.Clamp(0.4f * liftingBody.CL, 0.05f, 0.2f);
    }
}

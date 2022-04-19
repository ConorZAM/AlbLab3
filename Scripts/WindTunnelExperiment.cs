using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ExperimentManager))]
public class WindTunnelExperiment : MonoBehaviour
{
    // I've changed this because I want the name of the script - sorry!

    /* This script is in charge of collecting the following sets of data:
     *  - Cl vs alpha
     *  - Cm_cg, Cl, alpha and cg position? Not sure what this graph is...
     *  - Cm vs Cl with varying flap deflections
     *  - Cn vs Rudder deflection 
     */

    // On day I will perform the same reflection magic I used in the data save script
    // to create an experiment manager script which varies variables, calls functions, and records results

    // Properties of the aircraft which should be moved elsewhere
    public float wingArea, chord, density = 1.2f, q;

    // Manager handles the wiring of public things like rigid body and CG location
    ExperimentManager manager;

    // Global Wind sets the external wind velocity for all aero bodies in the scene, only gets the bodies
    // when the simulation starts though - don't add aero bodies while the simulation is running
    GlobalWind globalWind { get { return manager.globalWind; } }

    // This is the transform we'll position and rotate throughout the experiments
    Transform aircraftRoot { get { return manager.aircraftRb.transform.root; } }

    // Going to run through a range of angle of attack values - DEGREES!!!
    public float alphaMin, alphaMax;
    public int numberOfTestPoints;

    // The joint functions are on this script
    ForceBalance forceBalance;

    // Quick plot to see what data we're getting
    public AnimationCurve alphaClPlot;
    public AnimationCurve alphaCdPlot;
    public AnimationCurve clCmcgPlot;
    public AnimationCurve dragPolarPlot;

    // Outputs from the force balance
    public Vector3 measuredForceCoefficients, measuredTorqueCoefficients, measuredForce, measuredTorque;
    
    // Used for taring the force balance
    public Vector3 forceZero, torqueZero;

  
    //public AeroBody aeroBody;
    //public ThinAerofoilComponent thinAerofoil;
    //public float aeroBodyAlpha, aerobodyCL;
    //public Vector3 aeroBodyEarthFrameForce;

    public bool done;

    public float desiredAlpha;

    float alphaIncrement, alpha;
    int stepCount = 0;

    private void Awake()
    {
        manager = GetComponent<ExperimentManager>();
    }

    private void Reset()
    {
        manager = GetComponent<ExperimentManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        forceBalance = GetComponent<ForceBalance>();
        forceBalance.SetJointMode(ForceBalance.JointMode.Fixed);

        globalWind.Initialise();

        Debug.Log("Running Cl vs Alpha experiment");
        // Calculate the step size for alpha given the range and number of points
        alphaIncrement = (alphaMax - alphaMin) / numberOfTestPoints;
        alpha = alphaMin;

        // Clear the curve plots
        alphaClPlot = new AnimationCurve();
        alphaCdPlot = new AnimationCurve();
        dragPolarPlot = new AnimationCurve();
        clCmcgPlot = new AnimationCurve();

        StartCoroutine(GetClAlphaData());
    }

    private void FixedUpdate()
    {
        if(done)
            MeasureForces();
    }

    // We need a transformation between Unity and Aircraft axes - rotate and mirror image
    void MeasureForces()
    {
        // Earth and wind axes coincide - the joint reads in Unity's global frame
        measuredForce = forceBalance.ReadForce();
        measuredTorque = forceBalance.ReadTorque();
        measuredForceCoefficients = -measuredForce / (q * wingArea);
        // Drag acts in the negative Z direction so swap it round
        measuredForceCoefficients.z *= -1;
        measuredTorqueCoefficients = -measuredTorque / (q * wingArea * chord);
    }

    public IEnumerator GetClAlphaData()
    {
        // Wait for the physics to simulate
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < numberOfTestPoints; i++)
        {

            // Set the angle of attack by rotating the aircraft - note this isn't rotating about the CG
            SetAircraftRotation(alpha);

            // Re-tare the force balance - maybe not necessary to do with every rotation?
            // ----------------------------------------------------------
            //// Make sure no wind is blowing
            //globalWind.windSpeed = 0;
            //globalWind.SetWindVelocity();

            //// Wait for physics to simulate
            //yield return new WaitForFixedUpdate();

            //// Get the current forces on the joint so we can offset
            //forceZero = -manager.joint.currentForce;
            //torqueZero = -manager.joint.currentTorque;
            forceBalance.Tare();
            // ----------------------------------------------------------

            // Make sure the wind settings are correct
            globalWind.windAzimuth = 180;
            globalWind.windElevation = 0;
            globalWind.windSpeed = 10;
            q = 0.5f * density * globalWind.windSpeed * globalWind.windSpeed;

            // Apply the wind settings to all aero bodies in the scene
            globalWind.SetWindVelocity();

            // Wait for the physics to simulate
            yield return new WaitForFixedUpdate();

            // Measure the force acting on the joint
            MeasureForces();

            // Coefficients
            float Cl = measuredForceCoefficients.y;
            float Cd = measuredForceCoefficients.z;
            float Cm_cg = measuredTorqueCoefficients.x;

            // Add the measured data to the curve plots
            alphaClPlot.AddKey(alpha, Cl);
            alphaCdPlot.AddKey(alpha, Cd);

            // Only want the linear portion of the drag polar
            if (alpha < 9f)
            {
                clCmcgPlot.AddKey(Cl, Cm_cg);
                dragPolarPlot.AddKey(Cl, Cd);
            }

            // Increment the angle of attack for the next run
            alpha += alphaIncrement;
        }

        done = true;
    }

    public void SetAircraftRotation(Quaternion rotation)
    {
        forceBalance.RemoveJoint();
        aircraftRoot.rotation = rotation;
        forceBalance.AddJoint();
    }

    public void SetAircraftRotation(float _alpha)
    {
        forceBalance.RemoveJoint();
        aircraftRoot.rotation = Quaternion.Euler(-_alpha, 0, 0);
        forceBalance.AddJoint();
    }

}

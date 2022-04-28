using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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


    // Where to save the file
    private readonly string path = "Assets\\Unity Wind Tunnel Data.txt";

    [Header("Increase this value to slow down the experiment visuals")]
    [Range(1f, 100f)]
    public float slowDownFactor = 1f;

    // Properties of the aircraft which should be moved elsewhere
    [Header("Reference aircraft properties, used to calculate coefficients")]
    public float wingArea = 0.72f;
    public float meanAerodynamicChord = 0.233f, rho = 1.225f, q;

    [Header("Fixed height of the CG. Note, this script overrides the CG position")]
    public float cgHeight = -0.03f;
    public List<float> cgPositions = new() { -0.05f, -0.1f, 0f, 0.1f };
    // Manager handles the wiring of public things like rigid body and CG location
    FlightDynamicsLabManager Manager { get { return FlightDynamicsLabManager.Singleton(); } }

    // Global Wind sets the external wind velocity for all aero bodies in the scene, only gets the bodies
    // when the simulation starts though - don't add aero bodies while the simulation is running
    GlobalWind GlobalWind { get { return GlobalWind.Singleton(); } }

    // This is the transform we'll position and rotate throughout the experiments
    Transform AircraftRoot { get { return Manager.aircraftRb.transform.root; } }

    // Used to place the aircraft CG - don't forget to redo the joint positions!
    CentreOfMassManager CentreOfMassManager { get { return CentreOfMassManager.Singleton(); } }

    // Going to run through a range of angle of attack values - DEGREES!!!
    [Header("Independent variable ranges")]
    public float alphaMin;
    public float alphaMax;
    public int numberOfAlphaPoints;

    // CG Movements
    public float cgMin, cgMax;
    public int numberOfCgPoints;

    // Flap deflections
    public float rudderMin, rudderMax;
    public int numberOfRudderPoints;

    [Header("For listed variables, the default setting is the first value in the list")]
    public List<float> elevatorDeflections = new() { 0, 20, 40 };
    public List<float> flapDeflections = new() { 0, 20, 40 };

    // The joint functions are on this script
    ForceBalance forceBalance;

    // Outputs from the force balance
    public Vector3 measuredForceCoefficients, measuredTorqueCoefficients, measuredForce, measuredTorque;

    public bool done;

    // NOT INCLUDED AS IT IS NOT NECESSARY IN THIS EXPERIMENT - NEEDS MOVING TO THE STATIC EXPERIMENT
    // Used to set the rotation of the aircraft at runtime, editor script provides a button in inspector
    [HideInInspector]
    public float desiredAlpha;

    private void Awake()
    {
        //Manager = GetComponent<ExperimentManager>();
    }

    private void Reset()
    {
        //Manager = GetComponent<ExperimentManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

        GlobalWind.Initialise();

        Debug.Log("Running wind tunnel experiments");

        StartCoroutine(GetAircraftData());
    }

    private void FixedUpdate()
    {
        if (done)
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
        measuredTorqueCoefficients = -measuredTorque / (q * wingArea * meanAerodynamicChord);
        measuredTorqueCoefficients = CoordinateTransform.UnityToAircraftMoment(measuredTorqueCoefficients);
    }

    void SetCgPosition(float offset)
    {
        Manager.RemoveJoint();

        // Move the position marker
        CentreOfMassManager.SetCgPositionFromOffset(offset);

        Manager.AddJoint();
    }

    string GenerateFileHeader()
    {
        // File needs to go
        // alpha, Cd, Cl (for the range of flap deflections), Cm (range of elevator deflections)

        // Start with alpha
        string header = "alpha\t";

        // Append the Cl at flap deflections
        foreach (float deflection in flapDeflections)
        {
            header += "Cl for flap at " + deflection.ToString("F2") + "\t";
            header += "Cd for flap at " + deflection.ToString("F2") + "\t";
        }

        // Append Cm at elevator deflections
        foreach (float deflection in elevatorDeflections)
        {
            header += "Cm for elevator at " + deflection.ToString("F2") + "\t";
        }

        // Append Cm at cg positions
        foreach (float position in cgPositions)
        {
            header += "Cm for CG at " + position.ToString("F4") + "\t";
        }

        return header;
    }

    public IEnumerator GetAircraftData()
    {
        // Create the data file and put the header in
        FileStream f = File.Create(path);
        f.Close();

        string header = GenerateFileHeader();
        header += '\n';
        File.WriteAllText(path, header);



        float oldDt = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0.001f * slowDownFactor;

        // Calculate the step size for alpha given the range and number of points
        float alphaIncrement = (alphaMax - alphaMin) / (numberOfAlphaPoints - 1);
        float alpha = alphaMin;

        // Wait for the physics to simulate
        yield return new WaitForFixedUpdate();

        // Iterate over the angle of attack range
        for (int i = 0; i < numberOfAlphaPoints; i++)
        {

            /* In here we need to collect:
             *  - Cl, over a range of flap deployments
             *  - Cd, over a range of flap deployments
             *  - Cm_cg, over a range of elevator deflections
             *  - Cn (yaw) Don't think this is a concern now?
             */

            string data = alpha.ToString("F2") + "\t";

            // Set the angle of attack by rotating the aircraft - note this isn't rotating about the CG
            SetAircraftRotation(alpha);

            GlobalWind.windSpeed = 0;

            // Turn off the wind to tare the force balance
            GlobalWind.SetWindVelocity();
            yield return new WaitForFixedUpdate();

            // Re-tare the force balance - maybe not necessary to do with every rotation?
            forceBalance.Tare();

            // Make sure the wind settings are correct
            GlobalWind.windAzimuth = 180;
            GlobalWind.windElevation = 0;
            GlobalWind.windSpeed = 10;
            q = 0.5f * rho * GlobalWind.windSpeed * GlobalWind.windSpeed;

            // Apply the wind settings to all aero bodies in the scene
            GlobalWind.SetWindVelocity();

            // Set trim settings
            // The "trim" angles for the flaps are the first items in the lists
            Manager.controller.SetFlapDeflection(flapDeflections[0]);
            Manager.controller.SetElevatorDeflection(elevatorDeflections[0]);
            SetCgPosition(cgPositions[0]);

            // Iterate through the Cl values
            foreach (float deflection in flapDeflections)
            {
                // Set the flap deflection
                Manager.controller.SetFlapDeflection(deflection);

                // Wait for the physics to simulate
                yield return new WaitForFixedUpdate();

                // Measure the force acting on the joint
                MeasureForces();

                // Get the coefficients
                float Cl = measuredForceCoefficients.y;
                data += Cl.ToString("F4") + "\t";

                float Cd = measuredForceCoefficients.z;
                data += Cd.ToString("F4") + "\t";
            }

            // Revert to trim
            Manager.controller.SetFlapDeflection(flapDeflections[0]);

            // Iterate through the Cm values
            foreach (float deflection in elevatorDeflections)
            {
                // Set the flap deflection
                Manager.controller.SetElevatorDeflection(deflection);

                // Wait for the physics to simulate
                yield return new WaitForFixedUpdate();

                // Measure the force acting on the joint
                MeasureForces();

                // Get the coefficients
                float Cm_cg = measuredTorqueCoefficients.x;
                data += Cm_cg.ToString("F4") + "\t";
            }

            // Revert to trim
            Manager.controller.SetElevatorDeflection(elevatorDeflections[0]);

            // Iterate through the Cm values
            foreach (float position in cgPositions)
            {
                SetCgPosition(position);

                // Wait for the physics to simulate
                yield return new WaitForFixedUpdate();

                // Measure the force acting on the joint
                MeasureForces();

                // Get the coefficients
                float Cm_cg = measuredTorqueCoefficients.x;
                data += Cm_cg.ToString("F4") + "\t";
            }

            data += "\n";

            File.AppendAllText(path, data);
            // Increment the angle of attack for the next run
            alpha += alphaIncrement;
        }

        done = true;
        Time.fixedDeltaTime = oldDt;
        Debug.Log("Done.");
    }

    public void SetAircraftRotation(Quaternion rotation)
    {
        Manager.RemoveJoint();
        AircraftRoot.rotation = rotation;
        Manager.AddJoint();
    }

    public void SetAircraftRotation(float _alpha)
    {
        Manager.RemoveJoint();
        AircraftRoot.rotation = Quaternion.Euler(-_alpha, 0, 0);
        Manager.AddJoint();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
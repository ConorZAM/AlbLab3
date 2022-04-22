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
    public string path = "Unity Wind Tunnel Data.txt";

    // Properties of the aircraft which should be moved elsewhere
    public float wingArea = 0.8f, chord = 0.5f, density = 1.2f, q;
    public float cgHeight = -0.03f;

    // Manager handles the wiring of public things like rigid body and CG location
    ExperimentManager Manager { get { return ExperimentManager.Singleton(); } }

    // Global Wind sets the external wind velocity for all aero bodies in the scene, only gets the bodies
    // when the simulation starts though - don't add aero bodies while the simulation is running
    GlobalWind GlobalWind { get { return Manager.globalWind; } }

    // This is the transform we'll position and rotate throughout the experiments
    Transform AircraftRoot { get { return Manager.aircraftRb.transform.root; } }

    // Going to run through a range of angle of attack values - DEGREES!!!
    public float alphaMin, alphaMax;
    public int numberOfAlphaPoints;

    // CG Movements
    public float cgMin, cgMax;
    public int numberOfCgPoints;

    // Flap deflections
    public List<float> elevatorDeflections = new List<float> { 0, 20, 40 };
    public float rudderMin, rudderMax;
    public int numberOfRudderPoints;
    public List<float> flapDeflections = new List<float> { 0, 20, 40 };
    public List<float> cgPositions = new List<float> { -0.05f, -0.1f, 0f, 0.1f };

    // The joint functions are on this script
    ForceBalance forceBalance;

    // Outputs from the force balance
    public Vector3 measuredForceCoefficients, measuredTorqueCoefficients, measuredForce, measuredTorque;
    
    // Used for taring the force balance
    public Vector3 forceZero, torqueZero;


    public bool done;

    public float desiredAlpha;

    float alphaIncrement, alpha;
    int stepCount = 0;

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
        forceBalance = FindObjectOfType<ForceBalance>();
        forceBalance.SetJointMode(ForceBalance.JointMode.Fixed);

        GlobalWind.Initialise();

        Debug.Log("Running wind tunnel experiments");
        

        StartCoroutine(GetAircraftData());
    }

    private void FixedUpdate()
    {
        Debug.Log("uh");

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
        measuredTorqueCoefficients = -measuredTorque / (q * wingArea * chord);
        measuredTorqueCoefficients = CoordinateTransform.UnityToAircraftMoment(measuredTorqueCoefficients);
    }

    void SetCgPosition(float offset)
    {
        forceBalance.RemoveJoint();

        // Move the position marker
        Manager.centreOfGravity.position = Manager.leadingEdge.TransformPoint(new Vector3(0,cgHeight,offset));

        // Update the rigid body as well
        Manager.UpdateAircraftCg();

        forceBalance.AddJoint();
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
        Debug.Log(0);
        // Create the data file and put the header in
        FileStream f = File.Create(path);
        f.Close();

        

        string header = GenerateFileHeader();
        header += '\n';
        File.WriteAllText(path, header);

        

        float oldDt = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0.001f;

        // Calculate the step size for alpha given the range and number of points
        alphaIncrement = (alphaMax - alphaMin) / (numberOfAlphaPoints-1);
        alpha = alphaMin;

        Debug.Log("uh");

        // Wait for the physics to simulate
        yield return new WaitForFixedUpdate();

        Debug.Log(1);

        // Iterate over the angle of attack range
        for (int i = 0; i < numberOfAlphaPoints; i++)
        {

            /* In here we need to collect:
             *  - Cl, over a range of flap deployments
             *  - Cd
             *  - Cm_cg, over a range of elevator deflections
             *  - Cn (yaw)
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
            q = 0.5f * density * GlobalWind.windSpeed * GlobalWind.windSpeed;

            // Apply the wind settings to all aero bodies in the scene
            GlobalWind.SetWindVelocity();

            // Set trim settings
            // The "trim" angles for the flaps are the first items in the lists
            Manager.controller.SetFlapDeflection(flapDeflections[0]);
            Manager.controller.SetElevatorDeflection(elevatorDeflections[0]);
            SetCgPosition(cgPositions[0]);

            Debug.Log(2);


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

            Debug.Log(3);


            File.AppendAllText(path, data);
            // Increment the angle of attack for the next run
            alpha += alphaIncrement;

            Debug.Log(4);
        }

        done = true;
        Time.fixedDeltaTime = oldDt;
        Debug.Log("Done.");
    }

    public void SetAircraftRotation(Quaternion rotation)
    {
        forceBalance.RemoveJoint();
        AircraftRoot.rotation = rotation;
        forceBalance.AddJoint();
    }

    public void SetAircraftRotation(float _alpha)
    {
        forceBalance.RemoveJoint();
        AircraftRoot.rotation = Quaternion.Euler(-_alpha, 0, 0);
        forceBalance.AddJoint();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}

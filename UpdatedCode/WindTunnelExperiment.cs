using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WindTunnelExperiment : MonoBehaviour
{
    // This script has been updated!
    // It will now create two files, one for the alpha sweep and a separate file for the beta sweep
    // The alpha sweep runs with zero beta for all data points
    // The beta sweep uses the variable "alphaUsedInBetaSweep" for the angle of attack for all data points
    // The two files are created based on the path provided - they will automatically append "alpha" and "beta" appropriately
    
    // =================================================================
    // THE BETA SWEEP CAN BE TOGGLED ON AND OFF USING "performBetaSweep"
    // =================================================================
    
    /* This script is in charge of collecting the following sets of data:
     *  - Cl vs alpha
     *  - Cm_cg, Cl, alpha and cg position? Not sure what this graph is...
     *  - Cm vs Cl with varying flap deflections
     *  - Cn vs Rudder deflection 
     */

    // One day I will perform the same reflection magic I used in the data save script
    // to create an experiment manager script which varies variables, calls functions, and records results

    // Where to save the file
    public string path = "Assets\\Unity Wind Tunnel Data.txt";

    [Space(20), Tooltip("Increase this value to slow down the experiment visuals")]
    [Range(1f, 20f)]
    public float slowDownFactor = 1f;

    public int numberOfDataPoints = 100;

    // Properties of the aircraft which should be moved elsewhere
    [Header("Reference aircraft properties, used to calculate coefficients")]
    [Tooltip("Planform Area, S (m^2)")]
    public float wingArea = 0.72f;
    public float meanAerodynamicChord = 0.233f, rho = 1.225f;
    private float q;

    [Header("Fixed height (in metres) of the CG relative to the MAC leading edge.")]
    [Tooltip("The center of gravity height is fixed throughout this experiment as we are only concerned with the effect of longitudinal variation of the CG.")]
    public float cgHeight = -0.03f;

    // Going to run through a range of angle of attack and angle of sideslip values - DEGREES!!!
    [Header("Independent variable ranges")]
    public float alphaMin;
    public float alphaMax;    // Degrees
    public bool performBetaSweep;
    public float betaMin, betaMax;      // Degrees
    public float alphaUsedInBetaSweep;

    [Header("For listed variables, the default setting is the first value in the list")]
    public List<float> elevatorDeflections = new() { 0, 20, 40 };
    public List<float> flapDeflections = new() { 0, 20, 40 };
    public List<float> cgPositionsAsPercentageOfMac = new() { 25, 0, 50, 75 };
    // Manager handles the wiring of public things like rigid body and CG location
    FlightDynamicsLabManager Manager { get { return FlightDynamicsLabManager.Singleton(); } }

    // Global Wind sets the external wind velocity for all aero bodies in the scene, only gets the bodies
    // when the simulation starts though - don't add aero bodies while the simulation is running
    GlobalWind GlobalWind { get { return GlobalWind.Singleton(); } }

    // This is the transform we'll position and rotate throughout the experiments
    Transform AircraftRoot { get { return Manager.aircraftRb.transform.root; } }

    // Used to place the aircraft CG - don't forget to redo the joint positions!
    CentreOfMassManager CentreOfMassManager { get { return CentreOfMassManager.Singleton(); } }

  

    // The joint functions are on this script
    ForceBalance ForceBalance { get { return ForceBalance.Singleton(); } }

    // Outputs from the force balance
    [HideInInspector]
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
        ForceBalance.gameObject.SetActive(true);

        Debug.Log("Running wind tunnel experiments");

        StartCoroutine(GetAircraftAlphaData());
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
        measuredForce = ForceBalance.ReadForce();
        measuredTorque = ForceBalance.ReadTorque();
        measuredForceCoefficients = -measuredForce / (q * wingArea);
        // Drag acts in the negative Z direction so swap it round
        measuredForceCoefficients.z *= -1;
        measuredTorqueCoefficients = -measuredTorque / (q * wingArea * meanAerodynamicChord);
        measuredTorqueCoefficients = CoordinateTransform.UnityToAircraftMoment(measuredTorqueCoefficients);
    }

    void SetCgPosition(float offset)
    {
        Manager.SetCgPosition(offset);
    }

    string GenerateAlphaFileHeader()
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
            header += "Cl for elevator at " + deflection.ToString("F2") + "\t";
            header += "Cm for elevator at " + deflection.ToString("F2") + "\t";
        }

        // Append Cm at cg positions
        foreach (float position in cgPositionsAsPercentageOfMac)
        {
            header += "Cm for CG at " + position.ToString("F4") + "\t";
        }

        return header;
    }

    string GenerateBetaFileHeader()
    {
        // File needs to go
        // beta, Cd, Cl (for the range of flap deflections), Cm (range of elevator deflections)

        // Start with alpha
        string header = "beta\t";

        // Append the Cl at flap deflections
        foreach (float deflection in flapDeflections)
        {
            header += "Cl for flap at " + deflection.ToString("F2") + "\t";
            header += "Cd for flap at " + deflection.ToString("F2") + "\t";
        }

        // Append Cm at elevator deflections
        foreach (float deflection in elevatorDeflections)
        {
            header += "Cl for elevator at " + deflection.ToString("F2") + "\t";
            header += "Cm for elevator at " + deflection.ToString("F2") + "\t";
        }

        // Append Cm at cg positions
        foreach (float position in cgPositionsAsPercentageOfMac)
        {
            header += "Cm for CG at " + position.ToString("F4") + "\t";
        }

        return header;
    }

    public IEnumerator GetAircraftAlphaData()
    {
        Debug.Log("Starting Alpha Sweep");

        string alphaPath = path.Split('.')[0] + " alpha.txt";

        // Create the data file and put the header in
        FileStream f = File.Create(alphaPath);
        f.Close();

        string header = GenerateAlphaFileHeader();
        header += '\n';
        File.WriteAllText(alphaPath, header);



        float oldDt = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0.001f * slowDownFactor;

        // Calculate the step size for alpha given the range and number of points
        float alphaIncrement = (alphaMax - alphaMin) / (numberOfDataPoints - 1);
        float alpha = alphaMin;

        // Wait for the physics to simulate
        yield return new WaitForFixedUpdate();

        // Iterate over the angle of attack range
        for (int i = 0; i < numberOfDataPoints; i++)
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
            ForceBalance.Tare();

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
            SetCgPosition(cgPositionsAsPercentageOfMac[0]);

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
                float Cl = measuredForceCoefficients.y;
                data += Cl.ToString("F4") + "\t";

                float Cm_cg = measuredTorqueCoefficients.x;
                data += Cm_cg.ToString("F4") + "\t";
            }

            // Revert to trim
            Manager.controller.SetElevatorDeflection(elevatorDeflections[0]);

            // Iterate through the Cm values
            foreach (float position in cgPositionsAsPercentageOfMac)
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

            File.AppendAllText(alphaPath, data);
            // Increment the angle of attack for the next run
            alpha += alphaIncrement;
        }

        done = true;
        Time.fixedDeltaTime = oldDt;

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log("Alpha Sweep Done.");

        if (performBetaSweep)
        {
            StartCoroutine(GetAircraftBetaData());
        }
    }

    
    public IEnumerator GetAircraftBetaData()
    {
        Debug.Log("Starting Beta Sweep");

        string betaPath = path.Split('.')[0] + " beta.txt";

        // Create the data file and put the header in
        FileStream f = File.Create(betaPath);
        f.Close();

        string header = GenerateBetaFileHeader();
        header += '\n';
        File.WriteAllText(betaPath, header);

        // Set the time scale so the user can see each position in the sweep
        float oldDt = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0.001f * slowDownFactor;

        // Calculate the step size for beta given the range and number of points
        float betaIncrement = (betaMax - betaMin) / (numberOfDataPoints - 1);
        float beta = betaMin;

        // Wait for the physics to simulate
        yield return new WaitForFixedUpdate();

        // Iterate over the angle of attack range
        for (int i = 0; i < numberOfDataPoints; i++)
        {

            /* In here we need to collect:
             *  - Cl, over a range of flap deployments
             *  - Cd, over a range of flap deployments
             *  - Cm_cg, over a range of elevator deflections
             *  - Cn (yaw) Don't think this is a concern now?
             */

            string data = beta.ToString("F2") + "\t";

            // Set angles by rotating the aircraft - note this isn't rotating about the CG
            SetAircraftRotation(alphaUsedInBetaSweep, beta);

            GlobalWind.windSpeed = 0;

            // Turn off the wind to tare the force balance
            GlobalWind.SetWindVelocity();
            yield return new WaitForFixedUpdate();

            // Re-tare the force balance - maybe not necessary to do with every rotation?
            ForceBalance.Tare();

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
            SetCgPosition(cgPositionsAsPercentageOfMac[0]);

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
                float Cl = measuredForceCoefficients.y;
                data += Cl.ToString("F4") + "\t";

                float Cm_cg = measuredTorqueCoefficients.x;
                data += Cm_cg.ToString("F4") + "\t";
            }

            // Revert to trim
            Manager.controller.SetElevatorDeflection(elevatorDeflections[0]);

            // Iterate through the Cm values
            foreach (float position in cgPositionsAsPercentageOfMac)
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

            File.AppendAllText(betaPath, data);
            // Increment the angle of attack for the next run
            beta += betaIncrement;
        }

        done = true;
        Time.fixedDeltaTime = oldDt;

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log("Beta Sweep Done.");
    }

    public void SetAircraftRotation(Quaternion rotation)
    {
        Manager.RemoveJoint();
        AircraftRoot.rotation = rotation;
        Manager.AddJoint();
    }

    // Only set angle of attack
    public void SetAircraftRotation(float _alpha)
    {
        Manager.RemoveJoint();
        AircraftRoot.rotation = Quaternion.Euler(-_alpha, 0, 0);
        Manager.AddJoint();
    }

    // Set angle of attack and angle of sideslip
    public void SetAircraftRotation(float _alpha, float _beta)
    {
        Manager.RemoveJoint();
        AircraftRoot.rotation = Quaternion.Euler(-_alpha, -_beta, 0);
        Manager.AddJoint();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}

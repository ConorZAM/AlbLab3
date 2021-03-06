using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightDynamicsLabManager : MonoBehaviour
{
    /* The experiment manager is where all the magic is going to happen.
     * In this script we will need to handle:
     * - Data collection from a parameter sweep experiment in a wind tunnel setting
     * - Graphing of data in a gimbal style setting
     * - Scene transition to free flight mode
     */

    static FlightDynamicsLabManager _singleton;
    public static FlightDynamicsLabManager Singleton()
    {
        if (_singleton == null)
        {
            _singleton = FindObjectOfType<FlightDynamicsLabManager>();
        }
        return _singleton;
    }

    [Header("Select the settings for your experiment")]
    public ExperimentSettings Settings;
    public bool useGroundBasedPilotViewInFreeFlightMode = false;
    [Space(20)]
    [Range(-100f, 100f)]
    public float CgAsPercentageOfMac;
    float MacLength = 0.233f;
    float verticalCgOffset;

    [HideInInspector]
    public ConfigurableJoint joint;
    public Rigidbody aircraftRb;
    [Tooltip("Used to position the CG of the aircraft")]
    public Transform centreOfGravity;
    public Transform leadingEdge;
    public AircraftManager controller;
    private Transform Root { get { return aircraftRb.transform.root; } }

    

    public void SetCgPosition(float offset)
    {
        ForceBalance forceBalance = GetComponent<ForceBalance>();
        forceBalance.RemoveJoint();

        // Move the position marker
        centreOfGravity.position = leadingEdge.TransformPoint(new Vector3(0, centreOfGravity.localPosition.y, offset));

        // Update the rigid body as well
        UpdateAircraftCg();

        forceBalance.AddJoint();
    }

    public void DoExperimentSetup()
    {
        SetCgPosition(-MacLength * CgAsPercentageOfMac / 100f);

        // Set aircraft and camera positions
        Root.position = Settings.aircraftPosition;
        Camera.main.transform.position = Settings.cameraPosition;
        Camera.main.transform.eulerAngles = Settings.cameraEulerAngles;

        //If in free flight mode give the option to use the  ground based observer camera instead of the third person camera
        if (Settings.jointState == ExperimentSettings.JointState.Free & useGroundBasedPilotViewInFreeFlightMode==true)
        {
            Camera.main.enabled = false; // this is the third person camera
            GameObject.Find("Ground Observer Camera").SetActive(true); // enable the ground pilot camera
        }

        // Apply the joint
        switch (Settings.jointState)
        {
            case ExperimentSettings.JointState.Fixed:
                AddFixedJoint();
                break;
            case ExperimentSettings.JointState.Lateral:
                AddFixedJoint();
                // Allow roll only
                joint.angularZMotion = ConfigurableJointMotion.Free;
                break;
            case ExperimentSettings.JointState.Longitudinal:
                AddFixedJoint();
                // Allow pitch only
                joint.angularXMotion = ConfigurableJointMotion.Free;
                break;
            case ExperimentSettings.JointState.Gimbal:
                AddGimbalJoint();
                break;
            case ExperimentSettings.JointState.Free:
                RemoveJoint();
                FindObjectOfType<ForceBalance>().enabled = false;
                break;
            default:
                AddFixedJoint();
                break;
        }

        // Set the wind
        GlobalWind.Singleton().Initialise();
        GlobalWind.Singleton().windAzimuth = Settings.windAzimuth;
        GlobalWind.Singleton().windElevation = Settings.windElevation;
        GlobalWind.Singleton().windSpeed = Settings.windSpeed;
        GlobalWind.Singleton().SetWindVelocity();

        // Set the aircraft velocity
        aircraftRb.velocity = Settings.aircraftVelocity;

        // Enable the data manager script

        // Have to use the active Data Loggers object first
        GameObject loggers = GameObject.Find("Data Loggers");

        // Disable all children
        for (int i = 0; i < loggers.transform.childCount; i++)
        {
            loggers.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Enable correct data manager
        if (Settings.DataManagerName != "None")
        {
            // I don't have a clue why this works for disabled objects,
            // but gameObject.Find doesn't work...
            loggers.transform.Find(Settings.DataManagerName).gameObject.SetActive(true);
        }
    }


    public void AddFixedJoint()
    {
        AddConfigurableJoint();

        // Join the centre of mass to the world at the centre of mass location
        // This seems a bit weird but it's correct!
        joint.anchor = aircraftRb.centerOfMass;
        joint.connectedAnchor = aircraftRb.worldCenterOfMass;

        // Fixed in translation
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // Fixed in rotation
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
    }

    public void AddGimbalJoint()
    {
        AddConfigurableJoint();

        joint.anchor = aircraftRb.centerOfMass;
        joint.connectedAnchor = aircraftRb.worldCenterOfMass;

        // Fixed in translation
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // Free to rotate
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
    }

    public void AddConfigurableJoint()
    {
        joint = aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
        if (joint == null)
        {
            joint = aircraftRb.gameObject.AddComponent<ConfigurableJoint>();
        }
    }

    public void RemoveJoint()
    {
        joint = aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            DestroyImmediate(joint);
        }
    }


    public void UpdateAircraftCg()
    {
        aircraftRb.centerOfMass = aircraftRb.transform.InverseTransformPoint(centreOfGravity.position);
    }


    private void Awake()
    {
        GetSingleton();
    }

    private void Reset()
    {
        GetSingleton();
    }

    void GetSingleton()
    {
        if (_singleton != null && _singleton != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _singleton = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DoExperimentSetup();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
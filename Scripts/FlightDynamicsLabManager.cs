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
    
    //[Range(-100f, 100f)]
    //public float CgAsPercentageOfMac;
    //float MacLength = 0.233f;
    //float verticalCgOffset;

    [HideInInspector]
    public ConfigurableJoint joint;
    [Space(20)]
    public Rigidbody aircraftRb;
    [Tooltip("Used to position the CG of the aircraft")]
    public Transform centreOfGravity;
    public Transform leadingEdge;
    public AircraftManager controller;
    private Transform Root { get { return aircraftRb.transform.root; } }
    private CentreOfMassManager CentreOfMassManager { get { return CentreOfMassManager.Singleton(); } }



    public void SetCgPosition(float offset)
    {
        RemoveJoint();

        CentreOfMassManager.SetCgPositionFromOffset(offset);

        AddJoint();
    }

    Transform SetCamera()
    {
        Transform cam = null;
        bool useMain = Settings.cameraName.ToLower() == "main";

        // Go through all cameras, MiniCam tag is used for overlay cameras
        Camera[] cameras = FindObjectsOfType<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            // Check if we have the main camera
            if (useMain && cameras[i].CompareTag("MainCamera"))
            {
                cam = cameras[i].transform;
                cam.gameObject.SetActive(true);
                continue;
            }

            // If we're using mini cams skip their disable
            if (Settings.useMiniCams && cameras[i].CompareTag("MiniCam"))
            {
                continue;
            }

            if (cameras[i].gameObject.name == Settings.cameraName)
            {
                cam = cameras[i].transform;
                cam.gameObject.SetActive(true);
                continue;
            }

            // If we get this far, the current camera is not relevant and can be disabled
            cameras[i].gameObject.SetActive(false);
        }

        if (cam == null)
        {
            // Default to the main camera if we can't find the camera specified
            Debug.LogWarning("Camera " + Settings.cameraName + " could not be found. Make sure it is set active. Using main camera for now.");
            return Camera.main.transform;
        }

        return cam;

    }


    public void DoExperimentSetup()
    {
        CentreOfMassManager.SetCgPositionFromTransform();
        //SetCgPosition(-MacLength * CgAsPercentageOfMac / 100f);

        // Set the camera specified and get the transform for positioning
        Transform cameraTransform = SetCamera();

        // Set aircraft and camera positions
        Root.position = Settings.aircraftPosition;
        cameraTransform.position = Settings.cameraPosition;
        cameraTransform.eulerAngles = Settings.cameraEulerAngles;

        AddJoint();

        // Set the wind
        GlobalWind.Singleton().Initialise();
        GlobalWind.Singleton().windAzimuth = Settings.windAzimuth;
        GlobalWind.Singleton().windElevation = Settings.windElevation;
        GlobalWind.Singleton().windSpeed = Settings.windSpeed;
        GlobalWind.Singleton().SetWindVelocity();

        // Set the aircraft velocity
        aircraftRb.velocity = Settings.aircraftVelocity;

        // -------- Enable the data manager script --------

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

    public void AddJoint()
    {
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
                // Turn off the force balance because there is no longer a joint
                ForceBalance fb = FindObjectOfType<ForceBalance>();
                if(fb != null) fb.enabled = false;
                break;
            default:
                AddFixedJoint();
                break;
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

    private void Awake()
    {
        GetSingleton();
        DoExperimentSetup();
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

    private void FixedUpdate()
    {
        if (CentreOfMassManager.HasLocalPositionChanged())
        {
            RemoveJoint();
            CentreOfMassManager.SetCgPositionFromTransform();
            AddJoint();
        }
    }
}
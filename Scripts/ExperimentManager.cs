using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    /* The experiment manager is where all the magic is going to happen.
     * In this script we will need to handle:
     * - Data collection from a parameter sweep experiment in a wind tunnel setting
     * - Graphing of data in a gimbal style setting
     * - Scene transition to free flight mode
     */

    public enum ExperimentSetting
    {
        WindTunnel,
        Gimbal,
        FreeFlight
    }

    public ExperimentSetting experimentSetting;
    private ExperimentSetting previousExperimentSetting;

    ConfigurableJoint joint;
    public Rigidbody aircraftRb;

    private Vector3 aircraftPosition_WindTunnel;
    private Vector3 aircraftPosition_Gimbal;
    private Vector3 aircraftPosition_freeFlight;


    void AddFixedJoint()
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

    void AddGimbalJoint()
    {
        AddConfigurableJoint();

        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        // Fixed in translation
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // Free to rotate
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
    }

    void AddConfigurableJoint()
    {
        joint = aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
        if (joint == null)
        {
            joint = aircraftRb.gameObject.AddComponent<ConfigurableJoint>();
        }
    }

    void RemoveJoint()
    {
        joint = aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            DestroyImmediate(joint);
        }
    }


    private void OnValidate()
    {
        // Only update if the user has changed the experiment setting
        if (previousExperimentSetting != experimentSetting)
        {
            DoExperimentSetup();
        }
    }

    void DoExperimentSetup()
    {
        switch (experimentSetting)
        {
            case ExperimentSetting.WindTunnel:
                DoWindTunnelSetup();
                break;
            case ExperimentSetting.Gimbal:
                DoGimbalSetup();
                break;
            case ExperimentSetting.FreeFlight:
                DoFreeFlightSetup();
                break;
            default:
                break;
        }

        previousExperimentSetting = experimentSetting;
    }

    private void DoFreeFlightSetup()
    {
        // For free flight we want no joints attached to the aircraft
        // No data should be recorded or saved
        // Might still be worth having telemetry going to grapher - will see how expensive it is

        RemoveJoint();
        aircraftRb.transform.position = aircraftPosition_freeFlight;

    }

    private void DoGimbalSetup()
    {
        // Gimbal set up needs data from the aircraft to be sent over to grapher
        // The aircraft will be attached to a joint with fixed translation but free rotation
        // The wind is set by the user at/before runtime

        // Need to take the joint off so we can move the aircraft
        RemoveJoint();

        aircraftRb.transform.position = aircraftPosition_Gimbal;

        AddGimbalJoint();
    }

    private void DoWindTunnelSetup()
    {
        // The wind tunnel is a strange one, we might have a parameter sweep in this script or
        // that might be on another script to keep things organised. Either way, that's something
        // which shouldn't automatically happen - the user needs to configure some of those settings
        // so that they play a part in managing their data collection.
        // Also, the aircraft needs to be on a completely fixed joint, no translation or rotation

        // Need to take the joint off so we can move the aircraft
        RemoveJoint();

        aircraftRb.transform.position = aircraftPosition_WindTunnel;

        AddFixedJoint();
    }




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

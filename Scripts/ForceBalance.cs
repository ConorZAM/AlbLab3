using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FlightDynamicsLabManager))]
public class ForceBalance : MonoBehaviour
{
    [HideInInspector]
    public ConfigurableJoint joint;
    FlightDynamicsLabManager Manager { get { return FlightDynamicsLabManager.Singleton(); } }

    [Header("Force Readings")]
    public Vector3 totalForce;
    public Vector3 totalTorque;
    public Vector3 taredForce, taredTorque;
    public Vector3 zeroForce, zeroTorque;


    public enum JointMode
    {
        Fixed,
        Gimbal,
        Free
    }

    // Honestly I don't know which joint setter I'm using now... I need to make a JointSetter or something
    [HideInInspector]
    public JointMode jointMode;

    public void SetJointMode(JointMode _jointMode)
    {
        jointMode = _jointMode;

        AddJoint();
        UpdateAircraftCg();
        SetJointAnchor();

        switch (jointMode)
        {
            case JointMode.Fixed:
                SetJointFixed();
                break;
            case JointMode.Gimbal:
                SetJointGimbal();
                break;
            case JointMode.Free:
                SetJointFree();
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        totalForce = joint.currentForce;
        totalTorque = joint.currentTorque;
        taredForce = ReadForce();
        taredTorque = ReadTorque();
    }

    public void Tare()
    {
        SetJointMode(jointMode);
        //globalWind.windSpeed = 0;
        //globalWind.SetWindVelocity();

        // Run a physics update to get the forces on the joint
        Physics.autoSimulation = false;
        Physics.Simulate(Time.fixedDeltaTime);

        // Get the current forces on the joint so we can offset
        zeroForce = -joint.currentForce;
        zeroTorque = -joint.currentTorque;

        Physics.autoSimulation = true;

    }

    public Vector3 ReadForce()
    {
        return joint.currentForce + zeroForce;
    }

    public Vector3 ReadTorque()
    {
        return joint.currentTorque + zeroTorque;
    }

    void UpdateAircraftCg()
    {
        Manager.aircraftRb.centerOfMass = Manager.aircraftRb.transform.InverseTransformPoint(Manager.centreOfGravity.position);
    }

    private void SetJointAnchor()
    {
        joint.anchor = Manager.aircraftRb.centerOfMass;
        joint.connectedAnchor = Manager.aircraftRb.worldCenterOfMass;
    }

    private void SetJointFree()
    {
        // Free in translation
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;
        // Free in rotation
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
    }

    private void SetJointGimbal()
    {
        // Fixed in translation
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // Free in rotation
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
    }

    private void SetJointFixed()
    {
        // Fixed in translation
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        // Fixed in rotation
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
    }

    public void AddJoint()
    {
        if (joint != null)
        {
            return;
        }

        joint = Manager.aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
        if (joint == null)
        {
            joint = Manager.aircraftRb.gameObject.AddComponent<ConfigurableJoint>();
        }
        SetJointMode(jointMode);
    }

    public void RemoveJoint()
    {
        joint = Manager.aircraftRb.gameObject.GetComponent<ConfigurableJoint>();
        if (joint != null)
        {
            DestroyImmediate(joint);
        }
    }

    private void Reset()
    {
        //manager = GetComponent<ExperimentManager>();
    }

    private void Awake()
    {
        //manager = GetComponent<ExperimentManager>();
    }
}

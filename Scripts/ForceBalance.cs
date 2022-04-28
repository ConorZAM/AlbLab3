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


    private void FixedUpdate()
    {
        if (joint != null)
        {
            totalForce = joint.currentForce;
            totalTorque = joint.currentTorque;
            taredForce = ReadForce();
            taredTorque = ReadTorque();
        }
    }

    public void Tare()
    {
        if(joint == null)
        {
            Debug.LogWarning("Could not tare force balance, no joint was found.");
            return;
        }
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

}

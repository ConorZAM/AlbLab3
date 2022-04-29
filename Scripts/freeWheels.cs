using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freeWheels : MonoBehaviour
{

    public bool applyBrakes = false;

    public float motorTorque = 0.0000001f, brakeTorque = 1;

    WheelCollider[] wheelColliders;
    // Note that a small finite motor torque of >0 is require to allow the wheels to freewheel. This is a curiosity of the wheel collider model
    //a brake torque of 1 is sufficent to hold the Albatross model
    void Awake()
    {
        wheelColliders = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider w in wheelColliders)
        {
            w.motorTorque = motorTorque;
            if (applyBrakes) { w.brakeTorque = brakeTorque; }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (applyBrakes)
        //{
        //    foreach (WheelCollider w in wheelColliders)
        //    {
        //        w.brakeTorque = brakeTorque;
        //    }
        //}
    }
}
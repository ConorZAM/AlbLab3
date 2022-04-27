using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freeWheels : MonoBehaviour
{
    public float motorTorque = 0.0000001f, brakeTorque = 0;
    // Note that a small finite motor torque of >0 is require to allow the wheels to freewheel. This is a curiosity of the wheel collider model
    //a brake torque of 1 is sufficent to hold the Albatross model
    void Awake()
    {
        foreach (WheelCollider w in GetComponentsInChildren<WheelCollider>())
        {
            w.motorTorque = motorTorque;
            w.brakeTorque = brakeTorque;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
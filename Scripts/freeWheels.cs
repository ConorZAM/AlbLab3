using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freeWheels : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        foreach (WheelCollider w in GetComponentsInChildren<WheelCollider>())
             //w.motorTorque = 0.0000001f;
            w.brakeTorque = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

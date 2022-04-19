using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class feeWheels : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        foreach (WheelCollider w in GetComponentsInChildren<WheelCollider>())
            w.motorTorque = 0.000001f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    public AircraftManager aircraftManager;
    public Rigidbody rb;
    public Transform propellerHub;
    public float spinScale = 1;
    
    void FixedUpdate()
    {
    }

    public void ApplyThrust(float thrust)
    {
        Vector3 thrustVector = new Vector3(0, 0, thrust);
        rb.AddForceAtPosition(propellerHub.TransformDirection(thrustVector), propellerHub.position);
        propellerHub.Rotate(0, 0, -spinScale * thrust * Time.deltaTime);
    }
}

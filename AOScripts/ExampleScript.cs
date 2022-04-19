using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    // Data we want to collect
    public Vector3 velocity { get { return rb.velocity; } }
    public float time { get { return Time.fixedTime; } }

    // Rigid body we're measuring data from
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
}
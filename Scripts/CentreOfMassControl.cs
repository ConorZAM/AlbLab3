using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentreOfMassControl : MonoBehaviour
{
    public Vector3 localCg;
    public float cgRadius = 0.25f;
    Rigidbody rb;

    private void OnValidate()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if(rb == null)
            {
                Debug.LogWarning("No rigid body on game object: " + gameObject.name);
                return;
            }
        }

        rb.centerOfMass = localCg;
    }

    private void OnDrawGizmos()
    {
        if (rb)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(rb.worldCenterOfMass, cgRadius);
        }
    }
}

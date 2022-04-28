using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentreOfMassManager : MonoBehaviour
{
    static CentreOfMassManager _singleton;
    public static CentreOfMassManager Singleton()
    {
        if (_singleton == null)
        {
            _singleton = FindObjectOfType<CentreOfMassManager>();
        }
        return _singleton;
    }
    
    [HideInInspector]
    public float cgAsPercentageOfMac;
    private readonly float MAC_LENGTH = 0.233f;
    public Transform leadingEdge;
    public Rigidbody aircraftRb;
    public Vector3 CgPositionWorld { get { return transform.position; } }
    Vector3 lastLocalPosition;

    public void SetCgPositionFromTransform()
    {
        // Set the centre of mass locally relative to this object's position
        aircraftRb.centerOfMass = aircraftRb.transform.InverseTransformPoint(transform.position);

        UpdateCgInfo();
    }

    public void SetCgPositionFromOffset(float offset)
    {
        Vector3 relativeCg = leadingEdge.InverseTransformPoint(transform.position);
        relativeCg.z = offset;
        transform.position = leadingEdge.TransformPoint(relativeCg);

        SetCgPositionFromTransform();
    }

    private void UpdateCgInfo()
    {
        Vector3 relativeCg = leadingEdge.InverseTransformPoint(transform.position);
        cgAsPercentageOfMac = relativeCg.z / MAC_LENGTH;
        lastLocalPosition = transform.localPosition;
    }

    public bool HasLocalPositionChanged()
    {
        return lastLocalPosition != transform.localPosition;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            SetCgPositionFromTransform();
        }
    }
}

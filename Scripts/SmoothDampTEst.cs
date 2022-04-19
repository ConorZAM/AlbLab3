using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothDampTEst : MonoBehaviour
{
    public float maxSpeed, smoothTime;
    float yVelocity;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float newPosition = Mathf.SmoothDamp(transform.position.y, target.position.y, ref yVelocity, smoothTime,maxSpeed);
        transform.position = new Vector3(transform.position.x, newPosition, transform.position.z);
    }
}

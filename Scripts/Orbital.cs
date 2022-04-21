using UnityEngine;
using System.Collections;

public class Orbital : MonoBehaviour
{
    public Transform target;
    public bool followVerticalMotion = false;
    float followFactor;
    Vector3 lastPosition;
    Vector3 direction;
    float distance;

    Vector3 movement;
    Vector3 rotation;

    void Awake()
    {
        direction = new Vector3(0, 0, (target.position - transform.position).magnitude);
        transform.SetParent(target);
        lastPosition = Input.mousePosition;
    }

    void Update()
    {
        Vector3 mouseDelta = Input.mousePosition - lastPosition;
        if (Input.GetMouseButton(0))
            movement += new Vector3(mouseDelta.x * 0.1f, mouseDelta.y * 0.05f, 0F);
        movement.z += Input.GetAxis("Mouse ScrollWheel") * -0.1F;

        rotation += movement;
        rotation.x = rotation.x % 360.0f;
        rotation.y = Mathf.Clamp(rotation.y, -90F, 5F);

        direction.z = Mathf.Clamp(direction.z - movement.z * 50F, 1F, 180F);
        if (followVerticalMotion == false) followFactor = 0;
        else followFactor = 1;


        transform.position = Vector3.Scale(target.position, new Vector3(1, followFactor, 1)) + Quaternion.Euler(180F - rotation.y, rotation.x, 0) * direction;
        //take off vertical movement to show bob
        //float height = transform.parent.position.y;
        //Vector3 offset = new Vector3(0, height, 0);
        //transform.position -= offset;

        transform.LookAt(target.position);

        lastPosition = Input.mousePosition;
        movement *= 0.5F;

    }
}

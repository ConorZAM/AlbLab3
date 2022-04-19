using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignAxis : MonoBehaviour
{
    public Transform leftNode, rightNode;
    // Start is called before the first frame update
    public void SetUpAxis()
    {
        transform.position = leftNode.position;
        transform.rotation = Quaternion.LookRotation(-leftNode.position+ rightNode.position);
    }

}

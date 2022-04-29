using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
 

public class getWeight : MonoBehaviour
{
    ConfigurableJoint cj;
    public float weight;
    public TMP_Text mytext;

    // Start is called before the first frame update
    void Start()
    {
        cj = GetComponent<ConfigurableJoint>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mytext.text = (Mathf.Round(1* cj.currentForce.y)/1).ToString()+ " N";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroBodyGroup : MonoBehaviour
{
    public List<AeroBody> aeroBodyGroup;
    public float groupAspectRatio;
    // Start is called before the first frame update
    void Start()
    {
        float span = 0;
        float area = 0;
        for (int i = 0; i < aeroBodyGroup.Count; i++)
        {
            //span+=aeroBodyGroup[i].Aero
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

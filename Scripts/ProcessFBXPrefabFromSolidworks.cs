using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessFBXPrefabFromSolidworks : MonoBehaviour
{
    
    public void ProcessNow()
    {
        Material depron = Resources.Load<Material>("Materials/Depron");
        Material brownPaper = Resources.Load<Material>("Materials/BrownPaper");
        Material goldMetal = Resources.Load<Material>("Materials/GoldMetal");
        Material plywood = Resources.Load<Material>("Materials/Plywood");
        Material greenMetal = Resources.Load<Material>("Materials/GreenMetal");
        Material magentaMetal = Resources.Load<Material>("Materials/MagentaMetal");

        //Copy the prefab to a new object
        //var GOcopy=Instantiate(gameObject);

        //create new empty gameobject
        var goCopy = new GameObject();
        goCopy.name = gameObject.name;
        //goCopy.transform.localScale = gameObject.transform.localScale;
        goCopy.transform.localPosition = gameObject.transform.localPosition;


        Transform[] ts = gameObject.transform.GetComponentsInChildren<Transform>();
        GameObject rootGO = null;
        foreach (Transform t in ts)
        {
            if (t.name == "Root") 
            {
                rootGO = t.gameObject;
                break;
            }
         }

        var rootGOparentcopy= Instantiate(rootGO.transform.parent.gameObject, goCopy.transform);
        rootGOparentcopy.transform.localScale = gameObject.transform.localScale;
        rootGOparentcopy.transform.localPosition = Vector3.Scale( rootGOparentcopy.transform.localPosition, gameObject.transform.localScale);
        goCopy.transform.localScale = Vector3.one;


        //  //Create a new transform to copy the model to
        //  Transform tfDestination = new GameObject().transform;
        //  tfDestination.name = transform.name;
        //  tfDestination.parent = transform.parent;


        //  //remove any child that does not have the Root object as a child
        //  Transform[] ts = transform.GetComponentsInChildren<Transform>();
        //  GameObject rootGO=null;
        //  foreach (Transform t in ts)
        //  {
        //      //print(t.name);
        //      if (t.name == "Root")
        //      {
        //          rootGO = t.gameObject;
        //          //print("found root");
        //          break;
        //      }
        //      else Destroy(t.gameObject);
        //  }
        //  //change location of destination to match root
        //  //tfDestination.localPosition = rootGO.transform.parent.localPosition;
        //  tfDestination.localPosition = Vector3.zero;
        ////make a copy
        //GameObject GOcopy = Instantiate(rootGO);
        //  GOcopy.transform.name = "Root";
        //  GOcopy.transform.parent = tfDestination;
        //  tfDestination.localScale = transform.localScale; // needed because original prefab is scaled
        gameObject.SetActive(false);

        MeshRenderer[] mrArray = rootGOparentcopy.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in mrArray)
        {
            //print("Mesh render name = " + mr.name);
            if (mr.name.Substring(0, 4) == "depr") mr.material = depron;
            if (mr.name.Substring(0, 4) == "foam") mr.material = brownPaper;
            if (mr.name.Substring(0, 4) == "gold") mr.material = goldMetal;
            if (mr.name.Substring(0, 4) == "plyw") mr.material = plywood;
            if (mr.name.Substring(0, 4) == "gree") mr.material = greenMetal;
            if (mr.name.Substring(0, 4) == "mage") mr.material = magentaMetal;

        }
    }

    
}

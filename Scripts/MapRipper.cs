using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRipper : MonoBehaviour
{
    public GameObject maptile, mapTilePrefab;
    public Material mat;
    // Start is called before the first frame update
    public void RipNow()
    {
        
        
        var mt = Instantiate(mapTilePrefab);
        mt.name = "maptile1";
        Mesh newMesh = new Mesh();
        newMesh = maptile.GetComponent<MeshFilter>().mesh;
        Material newMaterial = new Material(mat);
        newMaterial = maptile.GetComponent<MeshRenderer>().material;

        mt.GetComponent<MeshFilter>().mesh = newMesh;
        mt.GetComponent<MeshRenderer>().material = newMaterial;
        mt.transform.position = maptile.transform.position;

    }

    
    
}

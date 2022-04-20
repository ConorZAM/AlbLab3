using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordinateTransform
{
    public static Vector3 UnityToAircraftDirection(Vector3 unity)
    {
        return new Vector3(unity.x, -unity.y, unity.z);
    }

    public static Vector3 UnityToAircraftMoment(Vector3 unity)
    {
        return new Vector3(-unity.x, unity.y, -unity.z);
    }
}

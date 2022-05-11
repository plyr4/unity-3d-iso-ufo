using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LookAtCamera
{
    public static void Look(Transform transform)
    {
        transform.LookAt(Camera.main.transform);
        Quaternion q = transform.rotation;
        transform.rotation = Quaternion.Euler(-q.eulerAngles.x, q.eulerAngles.y + 180, -q.eulerAngles.z);
    }
}

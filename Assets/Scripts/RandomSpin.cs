using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomSpin
{
    // TODO: refactor
    public static Vector3 GetRandomSpin()
    {
        float xzModifier = 3f;

        float i = Random.Range(1f, 10f);
        int sign = Random.Range(0, 2) == 0 ? -1 : 1;
        float xRotation = 10 + 2 * i * sign * xzModifier;

        sign = Random.Range(0, 2) == 0 ? -1 : 1;
        i = Random.Range(1f, 10f);
        float yRotation = 5 + 2 * i * sign;

        sign = Random.Range(0, 2) == 0 ? -1 : 1;
        i = Random.Range(1f, 10f);
        float zRotation = 10 + 2 * i * sign * xzModifier;

        var rot = new Vector3(xRotation, yRotation, zRotation);
        return rot.normalized;
    }
}

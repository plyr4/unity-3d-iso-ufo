using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObjectSaver : MonoBehaviour
{
    public void RemoveAndApplyBeamProperties(GameObject obj)
    {
#if UNITY_EDITOR
        ApplyBeamableProperties script = obj.GetComponent<ApplyBeamableProperties>();
        DestroyImmediate(script);
        PrefabUtility.ApplyPrefabInstance(obj, InteractionMode.AutomatedAction);
#endif
    }
}

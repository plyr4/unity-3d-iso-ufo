using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ExecuteAlways]
public class GlobalObjects : MonoBehaviour
{
    [SerializeField]
    public TetherJointScriptableObject _tetherJoint;

    [SerializeField]
    public ObjectSaverScriptableObject _objectSaver;

    private static GlobalObjects _instance;

    public static GlobalObjects Instance
    {
        get
        {
            // attempt to locate the singleton
            if (_instance == null)
            {
                _instance = (GlobalObjects)FindObjectOfType(typeof(GlobalObjects));
            }

            // create a new singleton
            if (_instance == null)
            {
                _instance = (new GameObject("GlobalObjects")).AddComponent<GlobalObjects>();
                _instance.Initialize();
            }

            // return singleton
            return _instance;
        }
    }

    private void Initialize()
    {
        InitTetherJointProperties();
        InitObjectSaver();
    }

    private void InitTetherJointProperties()
    {
        if (_tetherJoint == null) Debug.LogWarning("GlobalObjects initialization error: _tetherJoint:TetherJointScriptableObject not initialized");
    }

    private void InitObjectSaver()
    {
        if (_objectSaver == null) Debug.LogWarning("GlobalObjects initialization error: _objectSaver:ObjectSaverScriptableObject not initialized");
    }
}

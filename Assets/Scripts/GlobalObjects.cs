using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObjects : MonoBehaviour
{
    public static GlobalObjects _instance { get; private set; }

    [SerializeField]
    public ConfigurableJoint _tetherJoint;
    [SerializeField]
    public ObjectSaver _objectSaver;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        InitTetherJointProperties();
        InitObjectSaver();
        _instance = this;
    }

    public static GlobalObjects Instance()
    {
        if (_instance == null) return new GlobalObjects();
        _instance.InitTetherJointProperties();
        _instance.InitObjectSaver();
        return _instance;
    }

    private void InitTetherJointProperties()
    {
        // attempt to use the present component
        if (_tetherJoint == null) _tetherJoint = gameObject.GetComponent<ConfigurableJoint>();

        if (_tetherJoint == null) Debug.LogWarning("GlobalObjects initialization error: _tetherJoint:ConfigurableJoint not initialized");
    }

    private void InitObjectSaver()
    {
        // attempt to use the present component
        if (_objectSaver == null) _objectSaver = gameObject.GetComponent<ObjectSaver>();

        // apply a default in the case that nothing was provided
        if (_objectSaver == null) _objectSaver = gameObject.AddComponent<ObjectSaver>();

        if (_objectSaver == null) Debug.LogWarning("GlobalObjects initialization error: _objectSaver:ObjectSaver not initialized");
    }
}

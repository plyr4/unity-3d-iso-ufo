using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : MonoBehaviour
{
    public static GameConstants _instance { get; private set; }

    [SerializeField]
    public BeamAttributes _beamAttributes;
    [SerializeField]
    public ObjectSaver _objectSaver;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        InitBeamAttributes();
        InitObjectSaver();
        _instance = this;
    }

    public static GameConstants Instance()
    {
        if (_instance == null) return new GameConstants();
        return _instance;
    }

    private void InitBeamAttributes()
    {
        // attempt to use the present component
        if (_beamAttributes == null) _beamAttributes = gameObject.GetComponent<BeamAttributes>();

        // apply a default in the case that nothing was provided
        if (_beamAttributes == null) _beamAttributes = gameObject.AddComponent<BeamAttributes>().InitializeDefaults();
    }
    private void InitObjectSaver()
    {
        // attempt to use the present component
        if (_objectSaver == null) _objectSaver = gameObject.GetComponent<ObjectSaver>();
    }
}

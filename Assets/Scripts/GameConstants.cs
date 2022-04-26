using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : MonoBehaviour
{
    public static GameConstants _instance { get; private set; }

    [SerializeField]
    public BeamAttributes _beamAttributes;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        InitBeamAttributes();
        _instance = this;
    }


    public static GameConstants Instance()
    {
        if (_instance == null) return new GameConstants();
        return _instance;
    }

    private void InitBeamAttributes()
    {
        if (_beamAttributes == null) _beamAttributes = gameObject.GetComponent<BeamAttributes>();
        if (_beamAttributes == null) _beamAttributes = gameObject.AddComponent<BeamAttributes>().InitializeDefaults();
    }
}

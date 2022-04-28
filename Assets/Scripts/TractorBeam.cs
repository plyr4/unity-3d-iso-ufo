using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VLB;

public class TractorBeam : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField]
    private PlayerInputs _input;

    [Header("Beam Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private Transform _beamSource;
    [SerializeField]
    private Rigidbody _tetherAnchor;
    [Space]
    [SerializeField]
    [Range(0f, 1f)]
    private float _beamStrength;
    [SerializeField]
    private bool LockBeamStrength;
    [Space]
    [SerializeField]
    [Range(0.01f, 10f)]
    private float BeamStrengthGrowthRate = 2f;
    [SerializeField]
    private int MaxBeamableObjects = 50;
    [SerializeField]
    [Range(0.01f, 50f)]
    private float MaxBeamDepth = 20f;
    [SerializeField]
    [Range(0.01f, 10f)]
    private float MaxBeamRadius = 4f;
    [SerializeField]
    [Range(0.01f, 2f)]
    private float BeamRangeModifier = 1.4f;
    [SerializeField]
    [Range(0f, 1f)]
    private float StrengthToGrab = 0.95f;
    [SerializeField]
    private LayerMask GroundLayers;
    [SerializeField]
    private LayerMask BeamableLayers;
    private RaycastHit[] _inBeam;
    private Dictionary<int, BeamableObject> _tetheredBeamables;

    [Header("Light Settings")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private Light _beamSpotLight;
    [SerializeField]
    private VolumetricLightBeam _vlb;
    [Space]
    [SerializeField]
    private bool LockLightProperties = true;
    [Space]
    [SerializeField]
    private float VLBSpotAngleModifier = 0.7f;
    [SerializeField]
    private float BeamSpotLightSpotAngleModifier = 0.25f;
    [SerializeField]
    private float VLBRangeStrengthCoefficient = 0.5f;
    private Color _originalSpotLightColor;

    // cached beam properties
    private float _cachedTime;
    public float _beamRadius
    {
        get
        {
            if (_cachedTime != Time.time) UpdateBeamProperties();
            return _cachedRadius;
        }
    }
    private float _cachedRadius;

    public float _beamDepth
    {
        get
        {
            if (_cachedTime != Time.time) UpdateBeamProperties();
            return _cachedDepth;
        }
    }
    private float _cachedDepth;

    public float _beamAngle
    {
        get
        {
            if (_cachedTime != Time.time) UpdateBeamProperties();
            return _cachedAngle;
        }
    }
    private float _cachedAngle;

    // needed for PhysicsCones
    private Physics physics;

    private void Start()
    {
        // initialize player input used for tractor beam triggers
        InitializeInputs();

        // initialize lights if not explicitly set in the inspector
        InitializeLights();

        // initialize beamables and beam properties
        InitializeBeam();
    }

    private void InitializeBeam()
    {
        // initialize cone raycast hits
        _inBeam = new RaycastHit[0];

        // initialize beamable objects
        _tetheredBeamables = new Dictionary<int, BeamableObject>();

        // initialize beam properties
        UpdateBeamProperties();
    }

    private void Update()
    {
        UpdateBeamStrength();
    }

    private void FixedUpdate()
    {
        UpdateBeamProperties();
        ResetLightColors();
        if (_input.fire) FireBeam();
        if (!_input.fire) ReleaseBeam();
        if (_input.altFire) RetractBeam();
        AbsorbBeamables();
    }

    private void OnDrawGizmosSelected()
    {
        DrawBeamDepthRay();
        DrawBeamPickupSphere();
    }

    private void InitializeInputs()
    {
        if (_input == null) _input = GetComponent<PlayerInputs>();
        if (_input == null) _input = GetComponentInChildren<PlayerInputs>();
        if (_input == null) _input = transform.parent.GetComponent<PlayerInputs>();
    }

    private void InitializeLights()
    {
        // beam spot light
        if (_beamSpotLight == null) _beamSpotLight = GetComponent<Light>();
        if (_beamSpotLight == null) _beamSpotLight = GetComponentInChildren<Light>();
        if (_beamSpotLight == null) _beamSpotLight = transform.parent.GetComponent<Light>();

        _originalSpotLightColor = _beamSpotLight.color;

        // volumetric light beam
        if (_vlb == null) _vlb = GetComponent<VolumetricLightBeam>();
        if (_vlb == null) _vlb = GetComponentInChildren<VolumetricLightBeam>();
        if (_vlb == null) _vlb = transform.parent.GetComponent<VolumetricLightBeam>();
    }

    private void UpdateBeamStrength()
    {
        _beamStrength = Mathf.Clamp(_beamStrength += (_input.fire ? 1 : -1) * BeamStrengthGrowthRate * Time.deltaTime, 0f, 1f);
    }

    private void UpdateBeamProperties()
    {
        // last time cache was updated
        _cachedTime = Time.time;

        // depth of the beam
        _cachedDepth = GetBeamDepth();

        // radius of the beam capture cone
        _cachedRadius = MaxBeamRadius * _cachedDepth / MaxBeamDepth;

        // angle of the capture cone (does this change?)
        _cachedAngle = Mathf.Atan(_cachedDepth / _cachedRadius / MaxBeamRadius) * Mathf.Rad2Deg;

        // apply dynamic properties to beam lights
        if (LockLightProperties) ApplyLightProperties();
    }

    private void FireBeam()
    {
        // beam has no strength
        //    this saves on useless physics calls and potentially invalid math
        if (_beamStrength <= 0f) return;

        // control how much strength is required for the beam to function
        if (_beamStrength < StrengthToGrab) return;

        // use cone cast to capture objects in the beam cone
        _inBeam = physics.ConeCastAll(
            _beamSource.position, AdjustedBeamRadius(), BeamDirection(), AdjustedBeamDistance(), _beamAngle, BeamableLayers);

        // the beam cone contains objects
        if (_inBeam.Length > 0)
        {
            for (int i = _inBeam.Length - 1; i >= 0; i--)
            {
                // ignore the player
                if (_inBeam[i].collider.gameObject.tag == "Player") continue;

                BeamableObject _beamable = _inBeam[i].collider.gameObject.GetComponent<BeamableObject>();

                // ignore non-beamables
                if (_beamable == null) continue;

                // tether to the beamable if possible
                AttemptTether(_beamable);
            }
        }
    }

    private void ReleaseBeam()
    {
        // remove and untether any objects in the beam
        foreach (BeamableObject _beamable in _tetheredBeamables.Values.ToList())
        {
            AttemptUntether(_beamable);
        }
    }

    private void RetractBeam()
    {
        // pull up any objects in the beam
        foreach (BeamableObject _beamable in _tetheredBeamables.Values.ToList()) _beamable.RetractJoint();
    }

    private void AbsorbBeamables()
    {
        // absorb any retracted beamables
        foreach (BeamableObject _beamable in _tetheredBeamables.Values.ToList())
        {
            if (_beamable.Retracted())
            {
                AbsorbBeamable(_beamable);
            }
        }
    }

    private void AbsorbBeamable(BeamableObject beamable)
    {
        // untether the beamable
        AttemptUntether(beamable);

        // destroy the object
        Destroy(beamable.gameObject);
    }

    private void AttemptTether(BeamableObject beamable)
    {
        // if beam is full
        if (_tetheredBeamables.Count >= MaxBeamableObjects)
        {
            DenyTether(beamable);
            return;
        }

        // tether beamable
        if (!beamable.Tethered()) beamable.Tether(this);

        // add beamable to the beam
        AddGrabbedBeamable(beamable);
    }

    private void AttemptUntether(BeamableObject beamable)
    {
        // untether beamable
        if (beamable.Tethered()) beamable.Untether();

        // remove beamable from the beam
        RemoveGrabbedBeamable(beamable);
    }

    private void DenyTether(BeamableObject beamable)
    {
        Debug.Log("implement this with some static object wiggle or something");
    }

    private void AddGrabbedBeamable(BeamableObject beamable)
    {
        _tetheredBeamables[beamable.gameObject.GetInstanceID()] = beamable;
    }

    private void RemoveGrabbedBeamable(BeamableObject beamable)
    {
        _tetheredBeamables.Remove(beamable.gameObject.GetInstanceID());
    }

    private void ClearGrabbedBeamables(BeamableObject beamable)
    {
        _tetheredBeamables.Clear();
    }

    private float GetBeamDepth()
    {
        // sends Raycast to the terrain and returns either distance to hit or MaxBeamDepth
        RaycastHit hit;
        return Physics.Raycast(_beamSource.position, BeamDirection(), out hit, MaxBeamDepth, GroundLayers) ? hit.distance : MaxBeamDepth;
    }

    public Rigidbody GetTetherAnchor()
    {
        return _tetherAnchor;
    }

    private Vector3 BeamDirection()
    {
        return -_beamSource.up;
    }

    private float AdjustedBeamDistance()
    {
        return _beamDepth * _beamStrength;
    }

    private float AdjustedBeamRadius()
    {
        return _beamRadius * _beamStrength;
    }

    private void ResetLightColors()
    {
        _beamSpotLight.color = _originalSpotLightColor;
    }

    private void ApplyBeamColors()
    {
        // apply the beam-up color
        _beamSpotLight.color = GameConstants.Instance()._beamAttributes.BeamGrabSpotLightColor;
    }

    private void ApplyLightProperties()
    {
        // spot light range and angles
        if (_beamSpotLight != null)
        {
            // beam spot light should reach the ground
            _beamSpotLight.range = _cachedDepth * BeamRangeModifier;

            // spot angle should match the cone angle as close as possible
            _beamSpotLight.spotAngle = _cachedAngle * BeamSpotLightSpotAngleModifier;

            // the inner spot angle matches the outer spot angle
            _beamSpotLight.innerSpotAngle = _cachedAngle * BeamSpotLightSpotAngleModifier;
        }

        // volumetric light shaft range and angle
        if (_vlb != null)
        {
            // vlb light should reach the spot light distance
            float _range = (_beamSpotLight != null) ? _beamSpotLight.range : _cachedDepth * BeamRangeModifier;

            // vlb fall off should match the strength of the beam
            //    the "beam up effect"
            _vlb.fallOffEnd = _range * _beamStrength * VLBRangeStrengthCoefficient;
            _vlb.fallOffStart = _vlb.fallOffEnd;

            // vlb spot angle should match the cone angle as close as possible
            _vlb.spotAngle = _cachedAngle * VLBSpotAngleModifier;
        }
    }

    // draw the ray between the ship and the ground
    private void DrawBeamDepthRay()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_beamSource.position, BeamDirection() * AdjustedBeamDistance());
    }

    // draw the sphere that is used as a cone raycast radius
    private void DrawBeamPickupSphere()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((_beamSource.position) + BeamDirection() * AdjustedBeamDistance(), AdjustedBeamRadius());
    }
}

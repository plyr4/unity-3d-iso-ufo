using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VLB;
using TMPro;
public class TractorBeam : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField]
    private PlayerInputs _input;
    [Space]
    [Space]
    [Space]

    [Header("Events")]
    [SerializeField]
    private GameEvent _tetherEvent;
    [SerializeField]
    private GameEvent _untetherEvent;
    [SerializeField]
    private GameEvent _beamFullEvent;
    [SerializeField]
    private GameEvent _beamableFixedUpdateEvent;
    [SerializeField]
    private GameEvent _beamableUpdateEvent;
    [SerializeField]
    private GameEvent _absorbBeamableEvent;
    [Space]
    [Space]
    [Space]

    [Header("Beam Settings")]
    [SerializeField]
    private Transform _beamSource;
    [SerializeField]
    private Rigidbody _tetherAnchor;
    [Space]

    [SerializeField]
    private float _pickupPower;
    [SerializeField]
    [Range(0f, 1f)]
    private float _strength;
    [SerializeField]
    private bool LockStrength;
    [SerializeField]
    public bool LockFire;
    [SerializeField]
    private bool LockAltFire;
    [Space]

    [SerializeField]
    [Range(0.01f, 10f)]
    private float StrengthGrowthRate = 2f;
    [SerializeField]
    public int MaxBeamableObjects = 50;
    [SerializeField]
    [Range(0.01f, 5f)]
    private float AnchorDepthFactor = 0.75f;
    [SerializeField]
    [Range(0.01f, 25f)]
    public float RetractionSpeed = 4f;
    [SerializeField]
    [Range(0.01f, 50f)]
    private float MaxDepth = 25f;
    [SerializeField]
    [Range(0.01f, 10f)]
    private float MaxRadius = 2.8f;
    [SerializeField]
    [Range(0.01f, 2f)]
    private float RangeFactor = 1.4f;
    [SerializeField]
    [Range(0f, 1f)]
    private float StrengthToGrab = 0.2f;
    [SerializeField]
    public bool TrackBeamableObjectJointChanges = true;
    [SerializeField]
    public bool DrawJoints = false;
    [SerializeField]
    public Vector2 DrawJointWidth = new Vector2(0.02f, 0.08f);
    [SerializeField]
    public Material DrawJointMaterial;
    [SerializeField]
    private LayerMask GroundLayers;
    [SerializeField]
    private LayerMask BeamableLayers;
    private RaycastHit[] _inBeam;
    public Dictionary<int, Beamable> _tetheredBeamables;
    private LineRenderer _line;
    [Space]
    [Space]
    [Space]

    [Header("Light Settings")]
    [SerializeField]
    private bool LockLightProperties = false;
    [Space]

    [SerializeField]
    private float VLBSpotAngleFactor = 0.7f;
    [SerializeField]
    private float BeamSpotLightSpotAngleFactor = 0.25f;
    [SerializeField]
    private float VLBRangeStrengthFactor = 0.5f;
    [SerializeField]
    public Color GrabSpotLightColor = Color.cyan;
    private Color _originalSpotLightColor;
    private Light _beamSpotLight;
    private VolumetricLightBeam _vlb;

    // cached beam properties
    private float _cachedTime;
    public float _radius
    {
        get
        {
            if (_cachedTime != Time.time) UpdateBeamProperties();
            return _cachedRadius;
        }
    }
    private float _cachedRadius;

    public float _depth
    {
        get
        {
            if (_cachedTime != Time.time) UpdateBeamProperties();
            return _cachedDepth;
        }
    }
    private float _cachedDepth;

    public float _angle
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
        // initialize the source transform
        if (_beamSource == null) _beamSource = transform;

        // initialize cone raycast hits
        _inBeam = new RaycastHit[0];

        // initialize beamable objects
        _tetheredBeamables = new Dictionary<int, Beamable>();

        // initialize beam properties
        UpdateBeamProperties();
    }

    private void Update()
    {
        UpdateBeamStrength();
        UpdateBeamables();
    }

    private void FixedUpdate()
    {
        UpdateBeamProperties();
        FixedUpdateBeamables();
        ResetLightColors();
        if (Fire()) FireBeam();
        if (!Fire()) ReleaseBeam();
        if (AltFire()) RetractBeam();
        AbsorbBeamables();
    }

    private void OnDrawGizmosSelected()
    {
        GizmosDrawBeamDepthRay();
        GizmosDrawBeamPickupSphere();
        GizmosDrawTetherJoints();
    }

    private void InitializeInputs()
    {
        if (_input == null) _input = GetComponent<PlayerInputs>();
        if (_input == null) _input = GetComponentInChildren<PlayerInputs>();
        if (_input == null) _input = transform.parent.GetComponent<PlayerInputs>();


        if (_input == null) Debug.LogWarning("TractorBeam initialization error: _input:PlayerInputs not initialized");
    }

    private void InitializeLights()
    {
        // beam spot light
        if (_beamSpotLight == null) _beamSpotLight = GetComponent<Light>();
        if (_beamSpotLight == null) _beamSpotLight = GetComponentInChildren<Light>();
        if (_beamSpotLight == null) _beamSpotLight = transform.parent.GetComponent<Light>();

        if (_beamSpotLight != null) _originalSpotLightColor = _beamSpotLight.color;

        // volumetric light beam
        if (_vlb == null) _vlb = GetComponent<VolumetricLightBeam>();
        if (_vlb == null) _vlb = GetComponentInChildren<VolumetricLightBeam>();
        if (_vlb == null) _vlb = transform.parent.GetComponent<VolumetricLightBeam>();

        if (_beamSpotLight == null) Debug.LogWarning("TractorBeam initialization error: _beamSpotLight:Light not initialized");
        if (_vlb == null) Debug.LogWarning("TractorBeam initialization error: _vlb:VolumetricLightBeam not initialized");
    }

    private bool Fire()
    {
        return LockFire || _input.fire;
    }

    private bool AltFire()
    {
        return LockAltFire || _input.altFire;
    }

    private void UpdateBeamStrength()
    {
        _strength = Mathf.Clamp(_strength += (LockStrength ? 0 : (Fire() ? 1 : -1)) * StrengthGrowthRate * Time.deltaTime, 0f, 1f);
    }

    private void UpdateBeamProperties()
    {
        // last time cache was updated
        _cachedTime = Time.time;

        // depth of the beam
        _cachedDepth = GetBeamDepth();

        // radius of the beam capture cone
        _cachedRadius = MaxRadius * _cachedDepth / MaxDepth;

        // angle of the capture cone (does this change?)
        _cachedAngle = Mathf.Atan(_cachedDepth / _cachedRadius / MaxRadius) * Mathf.Rad2Deg;

        // apply dynamic properties to beam lights
        if (!LockLightProperties) ApplyDynamicLightProperties();
    }

    private void FireBeam()
    {
        // beam has no strength
        //    this saves on useless physics calls and potentially invalid math
        if (_strength <= 0f) return;

        // control how much strength is required for the beam to function
        if (_strength < StrengthToGrab) return;

        // apply visual effects
        ApplyBeamColors();

        // use cone cast to capture objects in the beam cone
        _inBeam = physics.ConeCastAll(
            _beamSource.position, Radius(), Direction(), Depth(), _angle, BeamableLayers);

        // the beam cone contains objects
        if (_inBeam.Length > 0)
        {
            for (int i = _inBeam.Length - 1; i >= 0; i--)
            {
                GameObject obj = _inBeam[i].collider.gameObject;

                if (IsObjectTethered(obj)) continue;

                // if beam is full
                if (_tetheredBeamables.Count >= MaxBeamableObjects)
                {
                    DenyTether(obj);
                    _beamFullEvent?.Invoke();
                    continue;
                }

                var size = Util.Round(MeshFilters.GetObjectLargestBound(obj), 10f);
                if (size > GetPickupPowerRounded())
                {
                    DenyTether(obj);
                    continue;
                }
                Tether(obj);
            }
        }
    }


    public float GetPickupPowerRounded()
    {
        return Util.Round(_pickupPower, 10f);
    }

    public void Tether(GameObject obj)
    {
        // destroy an existing tether
        Beamable beamable = new Beamable(obj, this);

        // add beamable to tethered dictionary
        AddGrabbedBeamable(beamable);

        _tetherEvent?.Invoke(this, beamable);
    }

    public void Untether(Beamable beamable)
    {
        // remove tether
        beamable.Untether();

        // remove beamable from tethered dictionary
        RemoveGrabbedBeamable(beamable);

        _untetherEvent?.Invoke(this, beamable);
    }


    public bool IsObjectTethered(GameObject obj)
    {
        return _tetheredBeamables.ContainsKey(obj.GetInstanceID());
    }

    private void ReleaseBeam()
    {
        // remove and untether any objects in the beam
        foreach (Beamable beamable in _tetheredBeamables.Values.ToList())
        {
            if (!beamable._retract) Untether(beamable);
        }
    }

    private void RetractBeam()
    {
        // pull up any objects in the beam
        foreach (Beamable beamable in _tetheredBeamables.Values.ToList()) beamable.Retract();
    }
    private bool IsHoldingUnretractedBeamables()
    {
        Beamable tetherBeamable = _tetheredBeamables.Values.FirstOrDefault(b => !b._retract);
        return tetherBeamable != null;
    }
    private void UpdateBeamables()
    {
        // draw or clear the beam line
        if (_tetheredBeamables.Count > 0 && IsHoldingUnretractedBeamables() && DrawJoints) _line = DrawTetherJoints.DrawBeamAnchorLine(this, gameObject, _line);
        else DrawTetherJoints.ClearJointLine(_line);

        foreach (Beamable beamable in _tetheredBeamables.Values.ToList())
        {
            beamable.HandleUpdate(this, _beamableUpdateEvent);
        }
    }
    private void FixedUpdateBeamables()
    {
        foreach (Beamable beamable in _tetheredBeamables.Values.ToList())
        {
            // joint broke
            if (beamable._tether == null)
            {
                Untether(beamable);
            }

            beamable.HandleFixedUpdate(this, _beamableFixedUpdateEvent);
        }
    }

    private void AbsorbBeamables()
    {
        // absorb any retracted beamables
        foreach (Beamable beamable in _tetheredBeamables.Values.ToList())
        {
            if (beamable._retract && IsRetracted(beamable._tether))
            {
                AbsorbBeamable(beamable);
                _pickupPower += MeshFilters.GetMeshFilterLargestBound(beamable._meshFilter) * 0.02f;
                _absorbBeamableEvent?.Invoke(this, beamable);
            }
        }
    }

    public bool IsRetracted(ConfigurableJoint joint)
    {
        // TODO: MAKE better
        // perhaps bring back the death sphere
        float diff = Vector3.Distance(joint.transform.position - joint.anchor, joint.connectedBody.transform.position);
        return diff <= 2.5f && joint.connectedAnchor.y <= 0.1f;
    }

    private void AbsorbBeamable(Beamable beamable)
    {
        // untether the beamable
        Untether(beamable);

        // destroy the object
        Destroy(beamable._gameObject);
    }

    private void DenyTether(GameObject obj)
    {
        // Debug.Log("implement this with some static object wiggle or something");
        // shake only the mesh somehow? to not interfere with anything else like colliders or physics
    }

    private void AddGrabbedBeamable(Beamable beamable)
    {
        _tetheredBeamables[beamable._gameObject.GetInstanceID()] = beamable;
    }

    private void RemoveGrabbedBeamable(Beamable beamable)
    {
        _tetheredBeamables.Remove(beamable._gameObject.GetInstanceID());
    }
    private float GetBeamDepth()
    {
        // sends Raycast to the terrain and returns either distance to hit or MaxDepth
        RaycastHit hit;
        return Physics.Raycast(_beamSource.position, Direction(), out hit, MaxDepth, GroundLayers) ? hit.distance : MaxDepth;
    }

    public Rigidbody GetTetherAnchor()
    {
        return _tetherAnchor;
    }

    private Vector3 Direction()
    {
        return -_beamSource.up;
    }

    private float Depth()
    {
        return _depth * _strength;
    }

    private float Radius()
    {
        return _radius * _strength;
    }


    public float AnchorDepth()
    {
        return _depth * AnchorDepthFactor;
    }

    private void ResetLightColors()
    {
        if (_beamSpotLight != null && _beamSpotLight.color != _originalSpotLightColor) _beamSpotLight.color = _originalSpotLightColor;
    }

    private void ApplyBeamColors()
    {
        // apply the beam-up color
        if (_beamSpotLight != null && _beamSpotLight.color != GrabSpotLightColor) _beamSpotLight.color = GrabSpotLightColor;
    }

    private void ApplyDynamicLightProperties()
    {
        // spot light range and angles
        if (_beamSpotLight != null)
        {
            // beam spot light should reach the ground
            _beamSpotLight.range = _cachedDepth * RangeFactor;

            // spot angle should match the cone angle as close as possible
            _beamSpotLight.spotAngle = _cachedAngle * BeamSpotLightSpotAngleFactor;

            // the inner spot angle matches the outer spot angle
            _beamSpotLight.innerSpotAngle = _cachedAngle * BeamSpotLightSpotAngleFactor;
        }

        // volumetric light shaft range and angle
        if (_vlb != null)
        {
            // vlb light should reach the spot light distance
            float _range = (_beamSpotLight != null) ? _beamSpotLight.range : _cachedDepth * RangeFactor;

            // vlb fall off should match the strength of the beam
            //    the "beam up effect"
            _vlb.fallOffEnd = _range * _strength * VLBRangeStrengthFactor;
            _vlb.fallOffStart = _vlb.fallOffEnd;

            // vlb spot angle should match the cone angle as close as possible
            _vlb.spotAngle = _cachedAngle * VLBSpotAngleFactor;
        }
    }

    // draw the ray between the ship and the ground
    private void GizmosDrawBeamDepthRay()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_beamSource.position, Direction() * Depth());
    }

    // draw the sphere that is used as a cone raycast radius
    private void GizmosDrawBeamPickupSphere()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((_beamSource.position) + Direction() * Depth(), Radius());
    }

    private void GizmosDrawTetherJoints()
    {
        if (_tetheredBeamables == null) return;
        foreach (Beamable beamable in _tetheredBeamables.Values.ToList())
        {
            // joint.anchor
            // should be in the center of the hanging object
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(beamable._gameObject.transform.position + beamable._gameObject.transform.TransformDirection(beamable._tether.anchor), 0.2f);

            // joint.connectedAnchor
            // should be in the beam
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(beamable._tether.connectedBody.transform.position + beamable._tether.connectedAnchor, 0.2f);
        }
    }
}

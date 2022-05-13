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
    [Header("Events")]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    private GameEvent _tetherEvent;
    [SerializeField]
    private GameEvent _untetherEvent;
    [SerializeField]
    private GameEvent _beamFullEvent;
    [SerializeField]
    private GameEvent _updateGUITetheredObjectEvent;
    [Header("3D UI")]
    [Space]
    [Space]
    [Space]
    public GameObject _lastTethered;
    [SerializeField]
    private TextMeshPro _beamStrengthUI;
    [SerializeField]
    private TextMeshPro _beamCapacityUI;


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
    private float _beamPickupPower;
    [SerializeField]
    [Range(0f, 1f)]
    private float _beamStrength;
    [SerializeField]
    private bool LockBeamStrength;
    [SerializeField]
    public bool LockFire;
    [SerializeField]
    private bool LockAltFire;
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
    public bool TrackBeamableObjectJointChanges = false;
    [SerializeField]
    private LayerMask GroundLayers;
    [SerializeField]
    private LayerMask BeamableLayers;
    private RaycastHit[] _inBeam;
    private Dictionary<int, Beamable> _tetheredBeamables;
    private LineRenderer _line;

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
        _tetheredBeamables = new Dictionary<int, Beamable>();

        // initialize beam properties
        UpdateBeamProperties();
    }

    private void Update()
    {
        UpdateBeamStrength();
        UpdateBeamables();

        // TODO: move to event system
        UpdateUI();
    }

    // TODO: move to event system
    private void UpdateUI()
    {

        // beam capacity
        // TODO: convert this to "weight" carried
        _beamCapacityUI.text = string.Format("{0}/{1}", _tetheredBeamables.Keys.Count, MaxBeamableObjects);
        if (_tetheredBeamables.Keys.Count >= MaxBeamableObjects)
        {
            _beamCapacityUI.color = Color.red;
        }
        else
        {
            _beamCapacityUI.color = Color.white;
        }
        _beamStrengthUI.text = string.Format("{0}m", GetPickupPowerRounded());

        // UpdateLastBeamed(_nextBeamable);
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
        DrawBeamDepthRay();
        DrawBeamPickupSphere();
        DrawTetherJoints();
    }

    private void DrawTetherJoints()
    {
        if (_tetheredBeamables == null) return;
        foreach (Beamable _beamable in _tetheredBeamables.Values.ToList())
        {
            // joint.anchor
            // should be in the center of the hanging object
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_beamable._gameObject.transform.position + _beamable._gameObject.transform.TransformDirection(_beamable._tether.anchor), 0.2f);

            // joint.connectedAnchor
            // should be in the beam
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_beamable._tether.connectedBody.transform.position + _beamable._tether.connectedAnchor, 0.2f);
        }
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
        _beamStrength = Mathf.Clamp(_beamStrength += (LockBeamStrength ? 0 : (Fire() ? 1 : -1)) * BeamStrengthGrowthRate * Time.deltaTime, 0f, 1f);
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

        // apply visual effects
        ApplyBeamColors();

        // use cone cast to capture objects in the beam cone
        _inBeam = physics.ConeCastAll(
            _beamSource.position, AdjustedBeamRadius(), BeamDirection(), AdjustedBeamDistance(), _beamAngle, BeamableLayers);

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
        return Util.Round(_beamPickupPower, 10f);
    }
    private GameObject _previous;
    public void Tether(GameObject obj)
    {
        // destroy an existing tether
        Beamable beamable = new Beamable(obj, this);

        // add beamable to tethered dictionary
        AddGrabbedBeamable(beamable);
        _lastTethered = obj;
        _tetherEvent?.Invoke(obj, beamable);
    }

    public void Untether(Beamable beamable)
    {
        // remove tether
        beamable.Untether();

        // remove beamable from tethered dictionary
        RemoveGrabbedBeamable(beamable);

        _untetherEvent?.Invoke(null, beamable);
    }


    public bool IsObjectTethered(GameObject obj)
    {
        return _tetheredBeamables.ContainsKey(obj.GetInstanceID());
    }

    private void ReleaseBeam()
    {
        // remove and untether any objects in the beam
        foreach (Beamable _beamable in _tetheredBeamables.Values.ToList())
        {
            if (!_beamable._retract) Untether(_beamable);
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
        if (_tetheredBeamables.Count > 0 && IsHoldingUnretractedBeamables() && GameConstants.Instance()._beamAttributes.BeamDrawJoints) _line = TetherJoints.DrawBeamAnchorLine(this, gameObject, _line);
        else TetherJoints.ClearJointLine(_line);

        foreach (Beamable beamable in _tetheredBeamables.Values.ToList()) beamable.HandleUpdate(this);
    }
    private void FixedUpdateBeamables()
    {   
        bool f = false;
        foreach (Beamable beamable in _tetheredBeamables.Values.ToList())
        {
            // joint broke
            if (beamable._tether == null)
            {
                Untether(beamable);
            }

            beamable.HandleFixedUpdate(this);

            if (beamable._gameObject == _lastTethered) _updateGUITetheredObjectEvent?.Invoke(null, beamable);
        }
    }

    private void AbsorbBeamables()
    {
        // absorb any retracted beamables
        foreach (Beamable _beamable in _tetheredBeamables.Values.ToList())
        {
            if (_beamable._retract && IsRetracted(_beamable._tether))
            {
                AbsorbBeamable(_beamable);
                _beamPickupPower += MeshFilters.GetMeshFilterLargestBound(_beamable._meshFilter) * 0.02f;
            }
        }
    }

    public bool IsRetracted(ConfigurableJoint joint)
    {
        // TODO: MAKE better
        // perhaps bring back the death sphere
        float diff = Vector3.Distance(joint.transform.position, joint.connectedBody.transform.position);
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
        if (_beamSpotLight != null && _beamSpotLight.color != _originalSpotLightColor) _beamSpotLight.color = _originalSpotLightColor;
    }

    private void ApplyBeamColors()
    {
        Color c = GameConstants.Instance()._beamAttributes.BeamGrabSpotLightColor;
        // apply the beam-up color
        if (_beamSpotLight != null && _beamSpotLight.color != c) _beamSpotLight.color = c;
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












    // BEAMABLE OBJECT GARBAGE

    public float AnchorDepth()
    {
        return _beamDepth * GameConstants.Instance()._beamAttributes.BeamGrabAnchorDepthCoefficient;
    }
}

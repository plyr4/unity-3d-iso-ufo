using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using VLB;

[RequireComponent(typeof(PlayerInputs))]
public class TractorBeam : MonoBehaviour
{
    private PlayerInputs _input;

    [SerializeField]
    private Light _beamSpotLight;

    [SerializeField]
    private VolumetricLightBeam _vlb;

    private Color _originalSpotLightColor, _originalVLBColor;

    float _cachedTime;

    private float _cachedRadius;
    public float _beamRadius
    {
        get
        {
            if (_cachedTime != Time.time) LoadDynamicBeamProperties();
            return _cachedRadius;
        }
    }

    private float _cachedDepth;
    public float _beamDepth
    {
        get
        {
            if (_cachedTime != Time.time) LoadDynamicBeamProperties();
            return _cachedDepth;
        }
    }

    private float _cachedAngle;
    public float _beamAngle
    {
        get
        {
            if (_cachedTime != Time.time) LoadDynamicBeamProperties();
            return _cachedAngle;
        }
    }

    [SerializeField]
    private float _beamStrength;
    [SerializeField]
    private float ConstantBeamStrength;

    //// TODO: improve

    private Physics physics;

    private RaycastHit[] hitsInBeam;
    int numGrabbed = 0;


    [SerializeField]
    private Dictionary<int, BeamableObject> grabbedObjects;


    ///// END TODO

    [SerializeField]
    public bool LockLightProperties = false;
    [SerializeField]
    private float VLBSpotAngleModifier = 1f;
    [SerializeField]
    private float BeamSpotLightSpotAngleModifier = 0.25f;
    [SerializeField]
    private float DestinationRadius;
    [SerializeField]
    private float DestinationDepth;

    [SerializeField]
    private float MaxDepth = 20f;
    [SerializeField]
    private float MaxBeamRadius = 4f;
    [SerializeField]
    private float BeamRangeModifier = 1.4f;
    [SerializeField]
    [Range(0f, 1f)]
    private float BeamStrengthGrabThreshold = 0.95f;
    [SerializeField]
    private float BeamStrengthGrowthRate = 2f;

    [SerializeField]
    private float BeamSpeed;

    [SerializeField]
    private float BeamPickupSpeed = 1f;
    [SerializeField]
    private LayerMask GroundLayerMask;

    [SerializeField]
    private LayerMask BeamableLayerMask;

    [SerializeField]
    public Rigidbody _tetherAnchor;



    public void LoadDynamicBeamProperties()
    {
        _cachedTime = Time.time;
        _cachedDepth = GetDepth();
        _cachedRadius = MaxBeamRadius * _cachedDepth / MaxDepth;
        _cachedAngle = Mathf.Atan(_cachedDepth / _cachedRadius / MaxBeamRadius) * Mathf.Rad2Deg;

        // apply dynamic properties to beam lights
        if (LockLightProperties && _beamSpotLight != null && _vlb != null)
        {
            // spot light range and angles
            _beamSpotLight.range = _cachedDepth * BeamRangeModifier;
            _beamSpotLight.spotAngle = _cachedAngle * BeamSpotLightSpotAngleModifier;
            _beamSpotLight.innerSpotAngle = _cachedAngle * BeamSpotLightSpotAngleModifier;

            // volumetric light shaft range and angle
            _vlb.spotAngle = _cachedAngle * VLBSpotAngleModifier;
            _vlb.fallOffEnd = _beamSpotLight.range * 0.5f * (_beamStrength);
            _vlb.fallOffStart = _vlb.fallOffEnd * 0.95f;
        }
    }

    private void Start()
    {
        // player input used for tractor beam triggers
        _input = GetComponent<PlayerInputs>();

        // fetch lights if not explicitly set in the inspector
        if (_beamSpotLight == null) _beamSpotLight = GetComponentInChildren<Light>();
        if (_vlb == null) _vlb = GetComponentInChildren<VolumetricLightBeam>();

        // if we dont have lights by now, we deserve to crash
        _originalSpotLightColor = _beamSpotLight.color;
        _originalVLBColor = _vlb.color;

        // initialize cone raycast hits
        hitsInBeam = new RaycastHit[100];

        // initialize beamable objects
        grabbedObjects = new Dictionary<int, BeamableObject>();

        // initialize beam properties
        LoadDynamicBeamProperties();
    }
    void FixedUpdate()
    {
        LoadDynamicBeamProperties();
        // reset cone hits before checking them
        // if (numGrabbed > 0)
        // {
        //     for (int i = numGrabbed - 1; i >= 0; i--)
        //     {
        //         BeamableObject beamable = hitsInBeam[i].collider.gameObject.GetComponent<BeamableObject>();
        //         if (beamable != null)
        //         {
        //             beamable.SetGrabbed(false);
        //         }
        //     }
        // }

        // attempt to reset beam
        _beamSpotLight.color = _originalSpotLightColor;
        _vlb.color = _originalVLBColor;

        // handle beam +/-
        if (_input.fire) _beamStrength += BeamStrengthGrowthRate * Time.deltaTime;
        else _beamStrength -= BeamStrengthGrowthRate * Time.deltaTime;
        _beamStrength = Mathf.Clamp(_beamStrength, 0f, 1f);

        if (ConstantBeamStrength != 0f) _beamStrength = ConstantBeamStrength;

        // check for trigger and strength
        // if (_input.fire && (_beamStrength >= BeamStrengthGrabThreshold || (ConstantBeamStrength != 0f && _beamStrength >= ConstantBeamStrength)))
        if (_input.fire && (_beamStrength >= BeamStrengthGrabThreshold || (ConstantBeamStrength != 0f && _beamStrength >= ConstantBeamStrength)))
        {
            _beamSpotLight.color = GameConstants.Instance()._beamAttributes.BeamGrabSpotLightColor;
            // _vlb.color = BeamUpColor;

            hitsInBeam = physics.ConeCastAll(transform.position, BeamRadius(), BeamDirection(), BeamDistance(), _beamAngle, BeamableLayerMask);
            numGrabbed = hitsInBeam.Length;

            if (numGrabbed > 0)
            {
                for (int i = numGrabbed - 1; i >= 0; i--)
                {
                    if (hitsInBeam[i].collider.gameObject.tag != "Player")
                    {
                        BeamableObject beamable = hitsInBeam[i].collider.gameObject.GetComponent<BeamableObject>();
                        if (beamable != null)
                        {
                            if (!beamable.Tethered()) beamable.Tether(this);
                            AddGrabbedBeamable(beamable);
                        }
                    }
                }
            }
        }

        // released beam
        if (!_input.fire)
        {

                foreach (int objectID in grabbedObjects.Keys.ToList())
                {
                    BeamableObject obj = grabbedObjects[objectID];
                    if (obj.Tethered()) obj.Untether();
                    RemoveGrabbedBeamable(obj);
                }
        }

        if (_input.altFire)
        {
            foreach (int objectID in grabbedObjects.Keys.ToList())
            {
                BeamableObject obj = grabbedObjects[objectID];
                obj.RetractJoint();
            }
        }

        foreach (int objectID in grabbedObjects.Keys.ToList())
        {
            BeamableObject obj = grabbedObjects[objectID];
            if (obj.Retracted())
            {
                grabbedObjects.Remove(objectID);
                Destroy(obj.gameObject);
            }
        }

        // int maxColliders = 20;
        // Collider[] hitColliders = new Collider[maxColliders];
        // int numColliders = Physics.OverlapSphereNonAlloc((_beamSpotLight.transform.position + BeamDirection() * DestinationDepth), DestinationRadius, hitColliders);
        // for (int i = numColliders - 1; i >= 0; i--)
        // {
        //     BeamableObject beamable = hitColliders[i].gameObject.GetComponent<BeamableObject>();
        //     if (beamable != null)
        //     {
        //         beamable.gameObject.SetActive(false);
        //         RemoveGrabbedBeamable(beamable);
        //         Destroy(beamable.gameObject);
        //     }
        // }
    }


    void AddGrabbedBeamable(BeamableObject beamable)
    {
        grabbedObjects[beamable.gameObject.GetInstanceID()] = beamable;
    }

    void RemoveGrabbedBeamable(BeamableObject beamable)
    {
        grabbedObjects.Remove(beamable.gameObject.GetInstanceID());
    }

    void ClearGrabbedBeamables(BeamableObject beamable)
    {
        grabbedObjects.Clear();
    }

    bool InGrabbedBeamables(BeamableObject beamable)
    {
        return grabbedObjects.ContainsKey(beamable.gameObject.GetInstanceID());
    }

    // sends Raycast to the terrain and returns either distance to hit or MaxDepth
    float GetDepth()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, BeamDirection(), out hit, MaxDepth, GroundLayerMask)) return hit.distance;
        return MaxDepth;
    }

    Vector3 BeamDirection()
    {
        return -transform.up;
    }

    float BeamDistance()
    {
        return _beamDepth * _beamStrength;
    }


    float BeamRadius()
    {
        return _beamRadius * _beamStrength;
    }

    void OnDrawGizmosSelected()
    {
        DrawBeamDepthRay();
        DrawBeamDestinationSphere();
        DrawBeamPickupSphere();
    }

    // draw the ray between the ship and the ground
    void DrawBeamDepthRay()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, BeamDirection() * BeamDistance());
    }

    // draw the sphere that is used as a cone raycast radius
    void DrawBeamPickupSphere()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((transform.position) + BeamDirection() * BeamDistance(), BeamRadius());
    }

    // draw the sphere that is used as the beam destination
    void DrawBeamDestinationSphere()
    {
        Gizmos.color = new Color(148f, 0f, 211f);
        Gizmos.DrawWireSphere((_beamSpotLight.transform.position + BeamDirection() * DestinationDepth), DestinationRadius);
    }
}

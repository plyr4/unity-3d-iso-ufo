using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using VLB;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
public class TractorBeam : MonoBehaviour
{
    private PlayerInputs _input;
    private PlayerInput _playerInput;
    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    [SerializeField]
    private Light _light;
    [SerializeField]
    private VolumetricLightBeam _vlb;
    private Color _originalSpotLightColor;
    private Color _originalVLBColor;
    [SerializeField]
    private Color beamUpColor;


    public float radius;
    public float depth;
    public float angle;

    public float BeamSpeed;
    public float DestinationRadius;
    public float DestinationDepth;
    private Physics physics;

    private RaycastHit[] coneHits;

    private void Start()
    {
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();
        _originalSpotLightColor = _light.color;
        _originalVLBColor = _vlb.color;
        coneHits = new RaycastHit[0];
    }

    void FixedUpdate()
    {
        // reset cone hits before checking them
        if (coneHits.Length > 0)
        {
            for (int i = coneHits.Length - 1; i >= 0; i--)
            {
                BeamableObject beamable = coneHits[i].collider.gameObject.GetComponent<BeamableObject>();
                if (beamable != null)
                {
                    beamable.SetGrabbed(false);
                }
            }
        }

        _light.color = _originalSpotLightColor;
        _vlb.color = _originalVLBColor;
        if (_input.fire)
        {
            _light.color = beamUpColor;
            _vlb.color = beamUpColor;

            coneHits = physics.ConeCastAll(transform.position, radius, -transform.up, depth, angle);

            if (coneHits.Length > 0)
            {
                for (int i = coneHits.Length - 1; i >= 0; i--)
                {
                    if (coneHits[i].collider.gameObject.tag != "Player")
                    {
                        BeamableObject beamable = coneHits[i].collider.gameObject.GetComponent<BeamableObject>();
                        if (beamable != null)
                        {
                            beamable.SetGrabbed(true);
                            coneHits[i].collider.gameObject.transform.position = Vector3.Lerp(coneHits[i].collider.gameObject.transform.position, _light.transform.position, BeamSpeed * Time.deltaTime);
                        }
                    }
                }
            }
            int maxColliders = 10;
            Collider[] hitColliders = new Collider[maxColliders];
            int numColliders = Physics.OverlapSphereNonAlloc((_light.transform.position + -transform.up * DestinationDepth), radius, hitColliders);
            for (int i = numColliders - 1; i >= 0; i--)
            {
                BeamableObject beamable = hitColliders[i].gameObject.GetComponent<BeamableObject>();
                if (beamable != null)
                {
                    beamable.gameObject.SetActive(false);
                }
            }

        }

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 direction = -transform.up;
        Gizmos.DrawRay(transform.position, direction * depth);
        Gizmos.DrawWireSphere((transform.position) + direction * depth, radius);

        // draw the destination
        Gizmos.DrawWireSphere((_light.transform.position + direction * DestinationDepth), DestinationRadius);
    }
}

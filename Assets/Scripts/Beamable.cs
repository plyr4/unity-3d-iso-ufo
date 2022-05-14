using UnityEngine;

public class Beamable
{
    public GameObject _gameObject;
    public ConfigurableJoint _tether;
    public bool _retract = false;
    public Rigidbody _rb;
    public float _originalMass;
    public MeshFilter _meshFilter;

    // TODO: optimize
    public LineRenderer _line;

    public Beamable(GameObject obj, TractorBeam beam)
    {
        // set internal object
        _gameObject = obj;

        // create joint tether
        _tether = TetherJoints.CreateTether(obj, beam);

        // ensure mesh colliders are convex
        MeshColliders.SetMeshCollidersToConvex(_gameObject);

        // physics properties
        _rb = _gameObject.GetComponent<Rigidbody>();
        _rb.isKinematic = false;
        _originalMass = _rb.mass;

        // mesh
        _meshFilter = _gameObject.GetComponent<MeshFilter>();

        // TODO: constants
        _gameObject.layer = 8;

        // TODO: variable spin factor
        var spin = RandomSpin.GetRandomSpin() * 10f;
        _rb.AddTorque(spin);
    }

    public void HandleUpdate(TractorBeam beam, GameEvent updateGameEvent)
    {
        // draw the beam
        if (beam.DrawJoints) _line = TetherJoints.DrawObjectJointLine(beam, _gameObject, _line, _tether);
        else TetherJoints.ClearJointLine(_line);

        updateGameEvent?.Invoke(beam, this);
    }

    public void HandleFixedUpdate(TractorBeam beam, GameEvent fixedUpdateGameEvent)
    {
        UpdateTether(beam);
        SlowVelocity(beam);
        fixedUpdateGameEvent?.Invoke(beam, this);
    }

    public void UpdateTether(TractorBeam beam)
    {
        if (_tether == null) return;
        if (beam.TrackBeamableObjectJointChanges) TetherJoints.UpdateConstantJointProperties(_tether, _retract);
        TetherJoints.UpdateJointProperties(_tether, beam, _retract);

        // this controls joint retraction
        // when fully retracted the beamable is absorbed
        if (_retract)
        {
            TetherJoints.RetractJoint(beam, _tether);
        }
    }

    public void SlowVelocity(TractorBeam beam)
    {
        _rb.velocity = _rb.velocity * 0.9f;
    }

    public void Retract()
    {
        _retract = true;
        _rb.mass = 1f;

        // TODO: constants
        _gameObject.layer = 9;
    }

    public void Untether()
    {
        // TODO: constants
        _gameObject.layer = 8;

        _rb.mass = _originalMass;
        TetherJoints.ClearJointLine(_line);
        if (_tether != null) GameObject.Destroy(_tether);
    }

    public float Size()
    {
        return Util.Round(MeshFilters.GetMeshFilterLargestBound(_meshFilter), 10f);
    }

    public GameObject DisplayObject()
    {
        // create a new display object and a child mesh object
        GameObject container = new GameObject("display object - container");
        GameObject meshObject = new GameObject("display object - mesh");

        // set parent for pivot centering
        meshObject.transform.SetParent(container.transform);
        meshObject.transform.localPosition = Vector3.zero;
        meshObject.transform.localScale = Vector3.one;

        // set rigidbody properties
        Rigidbody rb = container.AddComponent<Rigidbody>();

        // match mass
        rb.mass = _rb.mass;

        // match angular velocity
        rb.angularVelocity = _rb.angularVelocity;

        // freeze position
        rb.constraints = RigidbodyConstraints.FreezePosition;

        // clone the mesh into the display object
        Mesh originalMesh = _meshFilter.sharedMesh;
        Mesh clonedMesh = new Mesh();
        clonedMesh.name = "cloned display object";
        if (originalMesh.isReadable)
        {
            clonedMesh.vertices = originalMesh.vertices;
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.uv = originalMesh.uv;
        }

        // add the renderer and filter to the display object
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();

        // assign the mesh
        meshFilter.mesh = clonedMesh;

        // assign the material
        meshRenderer.material = _gameObject.GetComponent<MeshRenderer>().material;

        // return the container object
        return container;
    }
}


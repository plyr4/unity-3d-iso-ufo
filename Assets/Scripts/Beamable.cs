using UnityEngine;

public class Beamable
{
    public GameObject _gameObject;
    public ConfigurableJoint _tether;
    public bool _retract = false;
    public Rigidbody _rb;
    public float _originalMass;
    private Vector3[] _vertices;
    public float _currentScale;
    public float _goalScale;
    public MeshFilter _meshFilter;
    public float _weight;

    // TODO: optimize
    public LineRenderer _line;

    public Beamable(GameObject obj, TractorBeam beam)
    {
        // set internal object
        _gameObject = obj;

        // mesh
        _meshFilter = _gameObject.GetComponent<MeshFilter>();

        // rigidbody
        _rb = _gameObject.GetComponent<Rigidbody>();

        // create joint tether using the scriptable object
        // this assigns the configurable joint to _tether
        GlobalObjects.Instance._tetherJoint.CreateTether(beam, this);

        // ensure mesh colliders are convex
        MeshColliders.SetMeshCollidersToConvex(_gameObject);

        // physics properties
        _rb.isKinematic = false;
        _originalMass = _rb.mass;
        _weight = MeshFilters.GetMeshFilterLargestBound(_meshFilter);
        
        // TODO: most likely bugged
        // should start at 1f
        _currentScale =  1f;
        _goalScale =  ScaleFactor();
        _vertices = MeshFilters.BaseVertices(_meshFilter);

        // TODO: constants
        _gameObject.layer = 8;

        // TODO: variable spin factor
        var spin = RandomSpin.GetRandomSpin() * 10f;
        _rb.AddTorque(spin);
    }

    public void HandleUpdate(TractorBeam beam, GameEvent updateGameEvent)
    {
        // draw the beam
        if (beam.DrawJoints) _line = DrawTetherJoints.DrawObjectJointLine(beam, _gameObject, _line, _tether);
        else DrawTetherJoints.ClearJointLine(_line);

        // raise event
        updateGameEvent?.Invoke(beam, this);
    }

    public void HandleFixedUpdate(TractorBeam beam, GameEvent fixedUpdateGameEvent)
    {
        // update tether properties
        UpdateTether(beam);

        // slow the object
        SlowVelocity(beam);

        // raise event
        fixedUpdateGameEvent?.Invoke(beam, this);
    }

    public void UpdateTether(TractorBeam beam)
    {
        if (_tether == null) return;

        if (beam.TrackBeamableObjectJointChanges)
        {
            // apply constant properties
            GlobalObjects.Instance._tetherJoint.ApplyTetherJointProperties(_tether);

            // apply dynamic properties
            GlobalObjects.Instance._tetherJoint.UpdateDynamicTetherJointProperties(beam, this);
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

    public Vector3 ScaleMeshToFitSphere()
    {
        Vector3 scale = _gameObject.transform.localScale;



        return scale;
    }

    public void Untether()
    {
        // TODO: constants
        _gameObject.layer = 8;

        _rb.mass = _originalMass;

        // clear the line renderer
        DrawTetherJoints.ClearJointLine(_line);

        // detroy the tether joint
        if (_tether != null) GameObject.Destroy(_tether);
    }

    public float Size()
    {
        return Util.Round(MeshFilters.GetMeshFilterLargestBound(_meshFilter), 10f);
    }


    public float ScaleFactor()
    {
        Mesh mesh = _meshFilter.mesh;
        Bounds bounds = mesh.bounds;
        var szB = bounds.size;
        float largestMeshDimension = Mathf.Max(szB.x, Mathf.Max(szB.y, szB.z));
        return 1f / largestMeshDimension;
    }

    public void ScaleDown(float speed)
    {
        if (_goalScale > 1f) return;
        _currentScale = Mathf.Lerp(_currentScale, _goalScale, Time.deltaTime * speed);
        MeshFilters.ScaleVertices(_meshFilter,_currentScale ,  _vertices, false);
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTests : MonoBehaviour
{
    [SerializeField]
    private GameObject _testObjects;

    [SerializeField]
    public TractorBeam _tractorBeam;

    [SerializeField]
    public int NumberToTether;

    [SerializeField]
    public GameObject SmallObject;

    public void ToggleBeamLock()
    {
        _tractorBeam.LockFire = !_tractorBeam.LockFire;
    }
    
    public bool BeamIsLocked()
    {
        return _tractorBeam.LockFire;
    }

    public void TetherObjects(int num)
    {
        Debug.Log("tethering " + num);
        for (int i = 0; i < num; i++)
        {
            GameObject _object = Instantiate(SmallObject, Vector3.zero, Quaternion.identity);
            _object.name = string.Format("performance test ({0})", i);
            _object.transform.parent = _testObjects.transform;
            Vector3 depth = Vector3.down * 3f;
            Vector3 right = Vector3.right * (Random.Range(-3, 4));
            Vector3 forward = Vector3.forward * (Random.Range(-2, 3));
            _object.transform.position = _tractorBeam.gameObject.transform.position + depth + right + forward;
            _tractorBeam.Tether(_object);
        }
    }
}

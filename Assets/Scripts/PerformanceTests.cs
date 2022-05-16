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

    [SerializeField]
    public GameObject MediumObject;

    [SerializeField]
    public GameObject LargeObject;

    [SerializeField]
    public GameObject OffCenterPivotObject;

    public void TetherSmallObjects(int num)
    {
        Debug.Log("tethering " + num + " small objects ", SmallObject);
        for (int i = 0; i < num; i++)
        {
            // tether
            TetherObject(SmallObject, i);
        }
    }

    public void TetherMediumObjects(int num)
    {
        Debug.Log("tethering " + num + " medium objects ", MediumObject);
        for (int i = 0; i < num; i++)
        {
            // tether
            TetherObject(MediumObject, i);
        }
    }

    public void TetherLargeObjects(int num)
    {
        Debug.Log("tethering " + num + " large objects ", LargeObject);
        for (int i = 0; i < num; i++)
        {   
            // tether
            TetherObject(LargeObject, i);
        }
    }

    public void TetherRandomSizedObjects(int num)
    {
        GameObject[] objects = new GameObject[3] {
            SmallObject,
            MediumObject,
            LargeObject
        };

        Debug.Log("tethering " + num + " random sized objects " + objects);

        for (int i = 0; i < num; i++)
        {
            // random object from small, med, large
            int r = Random.Range(0, 3);

            // tether
            TetherObject(objects[r], i);
        }
    }


    public void TetherOffCenterPivotObjects(int num)
    {
        Debug.Log("tethering " + num + " off center pivot objects " + OffCenterPivotObject);

        for (int i = 0; i < num; i++)
        {
            // tether
            TetherObject(OffCenterPivotObject, i);
        }
    }

    public void TetherObject(GameObject obj, int index)
    {
        LockFire();

        GameObject _object = Instantiate(obj, Vector3.zero, Quaternion.identity);
        _object.name = string.Format("performance test ({0})", index);
        _object.transform.parent = _testObjects.transform;
        Vector3 depth = Vector3.down * 3f;
        Vector3 right = Vector3.right * (Random.Range(-3, 4));
        Vector3 forward = Vector3.forward * (Random.Range(-2, 3));
        _object.transform.position = _tractorBeam.gameObject.transform.position + depth + right + forward;
        _tractorBeam.Tether(_object);
    }

    public void RetractObjects()
    {
        Debug.Log("retracting tethered objects");

        // retract anything in the beams
        StartCoroutine(RetractWithDelay());
    }

    IEnumerator RetractWithDelay()
    {
        // lock alt fire to retract objects
        LockAltFire();

        // wait half second
        yield return new WaitForSeconds(0.5f);

        // release alt fire
        UnlockAltFire();

        yield return null;
    }

    public void ToggleBeamLock()
    {
        _tractorBeam.LockFire = !_tractorBeam.LockFire;
    }

    public void LockFire()
    {
        _tractorBeam.LockFire = true;
    }

    public bool BeamIsLocked()
    {
        return _tractorBeam.LockFire;
    }

    public void ToggleBeamShouldAbsorb()
    {
        _tractorBeam.ShouldAbsorbBeamables = !_tractorBeam.ShouldAbsorbBeamables;
    }
    
    public bool BeamShouldAbsorb()
    {
        return _tractorBeam.ShouldAbsorbBeamables;
    }

    public void LockAltFire()
    {
        _tractorBeam.LockAltFire = true;
    }

    public void UnlockAltFire()
    {
        _tractorBeam.LockAltFire = false;
    }
}

using UnityEngine;

public class LockRotation : MonoBehaviour
{
    Quaternion rot;
    [SerializeField]
    private bool lockX, lockY, lockZ;

    void Start()
    {
        rot = transform.localRotation;
    }

    void LateUpdate()
    {
        Quaternion current = transform.localRotation;
        transform.localRotation =
            new Quaternion(lockX ? rot.x : current.x, lockY ? rot.y : current.y, lockZ ? rot.z : current.z, rot.w);
    }
}

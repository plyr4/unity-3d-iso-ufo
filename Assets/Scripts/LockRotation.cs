using UnityEngine;

public class LockRotation : MonoBehaviour
{   
    [SerializeField]
    private bool global;
    Quaternion rot;
    [SerializeField]
    private bool lockX, lockY, lockZ;

    void Start()
    {
        rot = global ? transform.rotation : transform.localRotation;
    }

    void LateUpdate()
    {
        Quaternion current = global ? transform.rotation : transform.localRotation;
        if (global)
            transform.rotation = new Quaternion(lockX ? rot.x : current.x, lockY ? rot.y : current.y, lockZ ? rot.z : current.z, rot.w);
        else
            transform.localRotation = new Quaternion(lockX ? rot.x : current.x, lockY ? rot.y : current.y, lockZ ? rot.z : current.z, rot.w);
    }
}

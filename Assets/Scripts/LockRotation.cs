using UnityEngine;

public class LockRotation : MonoBehaviour
{
    [SerializeField]
    private bool UseGlobalRotation;
    Quaternion _initialRotation;
    [SerializeField]
    private bool lockX, lockY, lockZ;

    void Start()
    {
        // initialize the rotation to lock
        _initialRotation = UseGlobalRotation ? transform.rotation : transform.localRotation;
    }

    void LateUpdate()
    {
        // lock rotations
        Lock();
    }

    private void Lock()
    {
        // capture current rotation
        Quaternion current = UseGlobalRotation ? transform.rotation : transform.localRotation;

        // set the locked rotation
        Quaternion rotation =
            new Quaternion(lockX ? _initialRotation.x : current.x, lockY ? _initialRotation.y : current.y, lockZ ? _initialRotation.z : current.z, _initialRotation.w);
        
        // update rotation
        if (UseGlobalRotation)
            transform.rotation = rotation;
        else
            transform.localRotation = rotation;
    }
}

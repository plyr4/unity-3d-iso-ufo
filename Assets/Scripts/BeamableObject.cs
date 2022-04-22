using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BeamableObject : MonoBehaviour
{
    [SerializeField]
    private bool _grabbed;
    [SerializeField]
    private Rigidbody _rb;

    void Start() {
        _rb = GetComponent<Rigidbody>();
        _grabbed = false;
    }

    void Update()
    {   
        if (_grabbed) {
            _rb.useGravity = false;
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            _rb.isKinematic = false;
        } else {
            _rb.useGravity = true;
        }
    }

    
    public void SetGrabbed(bool newGrabbedInput) {
        _grabbed = newGrabbedInput;
    }
}

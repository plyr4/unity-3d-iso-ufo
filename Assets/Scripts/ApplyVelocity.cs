using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyVelocity : MonoBehaviour
{
    [SerializeField] public Vector3 Velocity;
    private Rigidbody _rb;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        _rb.velocity = Velocity;
    }
}

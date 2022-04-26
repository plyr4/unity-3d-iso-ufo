using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawJoint : MonoBehaviour
{   
    LineRenderer line;

    ConfigurableJoint _joint;
    // Start is called before the first frame update
    void Start()
    {
        _joint = GetComponent<ConfigurableJoint>();
        line = this.gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, _joint.connectedBody.gameObject.transform.position);
        line.material.color = Color.red;
        line.GetComponent<Renderer>().enabled = true;
    }
}

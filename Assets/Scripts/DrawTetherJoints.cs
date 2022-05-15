using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class DrawTetherJoints
{
    public static LineRenderer DrawBeamAnchorLine(TractorBeam beam, GameObject obj, LineRenderer line)
    {
        // initialize line renderer
        if (line == null) line = obj.GetComponent<LineRenderer>();
        if (line == null) line = obj.AddComponent<LineRenderer>();

        // set default line renderer properties
        line.enabled = true;
        line.startWidth = beam.DrawJointWidth.x;
        line.endWidth = beam.DrawJointWidth.y;
        line.material = beam.DrawJointMaterial;
        line.startColor = beam.GrabSpotLightColor;
        line.endColor = beam.GrabSpotLightColor;

        // create or update the draw points
        Vector3[] _drawTransforms = BeamAnchorLineTransforms(obj, beam);
        line.positionCount = _drawTransforms.Length;
        for (int i = 0; i < _drawTransforms.Length; i++)
        {
            line.SetPosition(i, _drawTransforms[i]);
        }
        return line;
    }

    public static LineRenderer DrawObjectJointLine(TractorBeam beam, GameObject obj, LineRenderer line, ConfigurableJoint joint)
    {
        // initialize line renderer
        if (line == null) line = obj.GetComponent<LineRenderer>();
        if (line == null) line = obj.AddComponent<LineRenderer>();

        // set default line renderer properties
        line.enabled = true;
        line.startWidth = beam.DrawJointWidth.x;
        line.endWidth = beam.DrawJointWidth.y;
        line.material = beam.DrawJointMaterial;
        line.startColor = beam.GrabSpotLightColor;
        line.endColor = beam.GrabSpotLightColor;

        // create or update the draw points
        Vector3[] _drawTransforms = JointObjectLineTransforms(obj, joint);
        line.positionCount = _drawTransforms.Length;
        for (int i = 0; i < _drawTransforms.Length; i++)
        {
            line.SetPosition(i, _drawTransforms[i]);
        }
        return line;
    }

    public static Vector3[] BeamAnchorLineTransforms(GameObject obj, TractorBeam beam)
    {
        return new Vector3[2] {
                obj.transform.position,
                obj.transform.position + (-Vector3.up * beam.AnchorDepth()),
            };
    }

    public static Vector3[] JointObjectLineTransforms(GameObject obj, ConfigurableJoint joint)
    {
        return new Vector3[2] {
                obj.transform.position + obj.transform.TransformDirection(joint.anchor),
                joint.connectedBody.transform.position + joint.connectedAnchor,
            };
    }

    public static void ClearJointLine(LineRenderer line)
    {
        if (line == null) return;
        line.enabled = false;
    }
}

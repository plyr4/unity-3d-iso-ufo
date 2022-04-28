using System.Collections.Generic;
using UnityEngine;

public static class PhysicsCones
{
    public static RaycastHit[] ConeCastAll(this Physics physics, Vector3 origin, float maxRadius,
        Vector3 direction, float maxDistance, float coneAngle, LayerMask layers)
    {
        // apply a sphere cast to capture any objects that potentially lie in the cone angle
        RaycastHit[] _sphereCastHits =
            Physics.SphereCastAll(origin - direction.normalized * maxRadius, maxRadius, direction, maxDistance, layers);


        // list to store objects only that lie in the cone angle
        List<RaycastHit> coneCastHits = new List<RaycastHit>();
        if (_sphereCastHits.Length > 0)
        {
            for (int i = 0; i < _sphereCastHits.Length; i++)
            {
                // where the object collided with the sphere cast
                Vector3 _hitPoint = _sphereCastHits[i].point;

                // direction to the point of contact
                Vector3 directionToHit = _hitPoint - origin;

                // angle to the point of contact
                float _hitAngle = Vector3.Angle(direction, directionToHit);

                // compare this angle with the cone angle
                if (_hitAngle < coneAngle)
                {
                    // object lies in the cone angle
                    coneCastHits.Add(_sphereCastHits[i]);
                }
            }
        }

        // return cone cast hits
        return coneCastHits.ToArray();
    }
}
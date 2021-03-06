using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ObjectRenderSnapable))]
public class ApplyBeamableProperties : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<ObjectRenderSnapable>().SnapEulerAngles = false;
        gameObject.layer = 8;
        RequireCollider();
        GlobalObjects.Instance._objectSaver?.RemoveAndApplyBeamProperties(gameObject);
    }

    void RequireCollider()
    {
        bool collider = false;
        if (gameObject.GetComponent<BoxCollider>() != null) collider =  true;
        if (gameObject.GetComponent<SphereCollider>() != null) collider =  true;
        if (gameObject.GetComponent<CapsuleCollider>() != null) collider =  true;
        MeshCollider mc = gameObject.GetComponent<MeshCollider>();
        if (mc != null)
        {
            mc.convex = true;
            collider =  true;
        }

        if (!collider) gameObject.AddComponent<BoxCollider>();
    }
}

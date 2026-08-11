using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AircraftInertiaOverride : MonoBehaviour
{
    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // approximate aircraft as a box using renderer bounds
        var renderers = GetComponentsInChildren<Renderer>();
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);

        Vector3 size = b.size;
        float m = rb.mass;

        // inertia of a box about its OWN center of mass
        float ix = (1f / 12f) * m * (size.y * size.y + size.z * size.z);
        float iy = (1f / 12f) * m * (size.x * size.x + size.z * size.z);
        float iz = (1f / 12f) * m * (size.x * size.x + size.y * size.y);

        rb.inertiaTensor = new Vector3(ix, iy, iz);
        rb.inertiaTensorRotation = Quaternion.identity;
    }
}
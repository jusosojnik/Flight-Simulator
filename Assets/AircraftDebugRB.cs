using UnityEngine;

public class AircraftDebugRB : MonoBehaviour
{
    public Rigidbody rb;

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        Debug.Log($"RB mass:{rb.mass}");
        Debug.Log($"RB COM local:{rb.centerOfMass}");
        Debug.Log($"RB worldCOM:{rb.worldCenterOfMass}");
        Debug.Log($"RB inertiaTensor:{rb.inertiaTensor}");
        Debug.Log($"RB inertiaTensorRotation:{rb.inertiaTensorRotation.eulerAngles}");
    }
}

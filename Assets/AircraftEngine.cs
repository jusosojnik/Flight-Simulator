using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AircraftEngine : MonoBehaviour
{
    [Header("Engine")]
    public float maxThrust = 20000f;
    public float throttleSpeed = 0.5f;

    [Range(-1f, 1f)]
    public float throttle;
    public float throttleInput;

    Rigidbody rb;
    Transform rbTransform;   // <-- ALWAYS use this for directions

    InputSystem_Actions controls;

    void OnEnable()
    {
        if (controls == null)
            controls = new InputSystem_Actions();

        controls.Aircraft.Throttle.performed += ctx => throttleInput = ctx.ReadValue<float>();
        controls.Aircraft.Throttle.canceled += _ => throttleInput = 0f;

        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rbTransform = rb.transform;  // cache once so we never touch child transforms
    }

    void FixedUpdate()
    {
        // Smooth throttle change
        throttle += throttleInput * throttleSpeed * Time.fixedDeltaTime;
        throttle = Mathf.Clamp(throttle, -1f, 1f);

        ApplyThrust();
    }

    void ApplyThrust()
    {
        // IMPORTANT:
        // Use Rigidbody orientation, NOT `transform.forward`
        // This guarantees thrust is aligned with the physics body.
        Vector3 thrustDirection = rbTransform.forward;

        Vector3 thrustForce = thrustDirection * (throttle * maxThrust);

        // Applied at center of mass automatically
        rb.AddForce(thrustForce, ForceMode.Force);

        // Debug visualization (draws from actual COM)
        Debug.DrawRay(rb.worldCenterOfMass, thrustDirection * 5f, Color.green);
    }
}
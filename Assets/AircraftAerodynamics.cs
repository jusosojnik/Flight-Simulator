// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class AircraftAerodynamics : MonoBehaviour
// {
//     [Header("Wing")]
//     public float wingArea = 16f;
//     public float airDensity = 1.225f;

//     [Header("Lift (simple AoA-based)")]
//     public float cl0 = 0.2f;              // baseline lift at 0 AoA (arcade-friendly)
//     public float cla = 5.5f;              // lift slope (per radian)
//     public float stallAngleDeg = 18f;     // stall onset

//     [Header("Drag")]
//     public float cd0 = 0.03f;
//     public float inducedDrag = 0.08f;

//     [Header("Sideslip stability (stops 'random lift' in turns)")]
//     public float sideForce = 1.5f;        // pushes against sideways slip
//     public float yawStability = 1.2f;     // yaw damping from sideslip

//     [Header("Damping")]
//     public float angularDamping = 1.5f;
//     public float speedAngularDamping = 0.02f;

//     Rigidbody rb;

//     void Awake() => rb = GetComponent<Rigidbody>();

//     void FixedUpdate()
//     {
//         Vector3 vWorld = rb.linearVelocity;          // <-- use rb.velocity (not linearVelocity)
//         float speed = vWorld.magnitude;
//         if (speed < 0.5f) return;

//         Vector3 velDir = vWorld / speed;
//         Vector3 vLocal = transform.InverseTransformDirection(vWorld);

//         // AoA (pitch) and sideslip (yaw) in radians
//         float aoa = Mathf.Atan2(-vLocal.y, Mathf.Max(0.1f, vLocal.z)); // rad
//         float beta = Mathf.Atan2(vLocal.x, Mathf.Max(0.1f, vLocal.z)); // rad

//         float q = 0.5f * airDensity * speed * speed;

//         // Reduce aero when not moving mostly forward (prevents nonsense lift)
//         float forwardness = Mathf.Clamp01(Vector3.Dot(transform.forward, velDir));

//         // Stall factor
//         float stallAoa = stallAngleDeg * Mathf.Deg2Rad;
//         float aoaAbs = Mathf.Abs(aoa);
//         float stallT = Mathf.InverseLerp(stallAoa, stallAoa * 2f, aoaAbs);
//         float stallFactor = Mathf.Lerp(1f, 0.25f, stallT);

//         // Lift coefficient
//         float cl = cl0 + cla * aoa;
//         cl = Mathf.Clamp(cl, -1.8f, 1.8f);

//         // Stabilize lift direction by removing sideways velocity (huge for turning feel)
//         Vector3 vNoSlip = Vector3.ProjectOnPlane(vWorld, transform.right);
//         if (vNoSlip.sqrMagnitude < 0.01f) vNoSlip = vWorld;

//         Vector3 airflow = -vNoSlip.normalized;

//         // Lift direction (correct cross order)
//         Vector3 liftVec = Vector3.Cross(transform.right, airflow);
//         if (liftVec.sqrMagnitude > 1e-6f)
//         {
//             Vector3 liftDir = liftVec.normalized;

//             float lift = q * wingArea * cl * stallFactor * forwardness;
//             rb.AddForce(liftDir * lift, ForceMode.Force);
//         }

//         // Drag (parasitic + induced)
//         float cd = cd0 + inducedDrag * cl * cl;
//         float drag = q * wingArea * cd;
//         rb.AddForce(-velDir * drag, ForceMode.Force);

//         // Side force: kills sideways skid so lift doesn’t go wild in turns
//         float side = q * wingArea * (-beta * sideForce) * forwardness;
//         rb.AddForce(transform.right * side, ForceMode.Force);

//         // Damping: stops tumble / fishtail
//         float damp = angularDamping + speed * speedAngularDamping;
//         rb.AddTorque(-rb.angularVelocity * damp, ForceMode.Force);

//         // Extra yaw stability from sideslip
//         rb.AddTorque(transform.up * (-beta * yawStability * q * 0.001f), ForceMode.Force);
//     }
// }


using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AircraftAerodynamics : MonoBehaviour
{
    [Header("Wing")]
    public float wingArea = 16f;
    public float airDensity = 1.225f;

    [Header("Lift (AoA-based)")]
    public float cl0 = 0.25f;              // baseline lift at 0 AoA (raise for slower flight)
    public float cla = 5.5f;               // lift slope (per radian)
    public float stallAngleDeg = 24f;      // higher = more forgiving slow flight

    [Tooltip("How much lift remains after stall. 0.25 = harsh, 0.45 = forgiving.")]
    [Range(0.1f, 0.8f)] public float postStallLift = 0.45f;

    [Header("Drag")]
    public float cd0 = 0.02f;              // base drag
    public float inducedDrag = 0.06f;      // drag from lift^2

    [Header("Auto Flaps (helps low speed flight)")]
    public bool autoFlaps = true;
    public float flapsDeploySpeed = 35f;   // m/s: below this, flaps gradually deploy
    public float flapsLiftBonus = 0.35f;   // added to cl0 at full flaps
    public float flapsDragBonus = 0.04f;   // added to cd0 at full flaps

    [Header("Sideslip stability")]
    public float sideForce = 1.5f;
    public float yawStability = 1.2f;

    [Header("Damping")]
    public float angularDamping = 1.5f;
    public float speedAngularDamping = 0.02f;

    [Header("Takeoff helper")]
    [Tooltip("Prevents lift from dropping to 0 when you're rotating / slightly sideways.")]
    [Range(0f, 0.5f)] public float minForwardness = 0.15f;

    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        Vector3 vWorld = rb.linearVelocity;
        float speed = vWorld.magnitude;
        if (speed < 0.5f) return;

        Vector3 velDir = vWorld / speed;
        Vector3 vLocal = transform.InverseTransformDirection(vWorld);

        // AoA (pitch) and sideslip (yaw) in radians
        float aoa = Mathf.Atan2(-vLocal.y, Mathf.Max(0.1f, vLocal.z));
        float beta = Mathf.Atan2(vLocal.x, Mathf.Max(0.1f, vLocal.z));

        float q = 0.5f * airDensity * speed * speed;

        // Forwardness gating (but don't let it go to 0 during takeoff rotation)
        float forwardness = Mathf.Clamp01(Vector3.Dot(transform.forward, velDir));
        forwardness = Mathf.Max(forwardness, minForwardness);

        // Auto flaps amount (0..1)
        float flaps = 0f;
        if (autoFlaps && flapsDeploySpeed > 0.1f)
        {
            // Full flaps at 0 m/s, 0 flaps at/above deploy speed (smooth)
            flaps = Mathf.Clamp01((flapsDeploySpeed - speed) / flapsDeploySpeed);
            flaps = flaps * flaps * (3f - 2f * flaps); // SmoothStep
        }

        // Stall factor
        float stallAoa = stallAngleDeg * Mathf.Deg2Rad;
        float aoaAbs = Mathf.Abs(aoa);
        float stallT = Mathf.InverseLerp(stallAoa, stallAoa * 2f, aoaAbs);
        float stallFactor = Mathf.Lerp(1f, postStallLift, stallT);

        // Lift coefficient (with flaps boosting baseline lift)
        float cl = (cl0 + flaps * flapsLiftBonus) + cla * aoa;
        cl = Mathf.Clamp(cl, -1.8f, 1.8f);

        // Stabilize lift direction by removing sideways velocity component
        Vector3 vNoSlip = Vector3.ProjectOnPlane(vWorld, transform.right);
        if (vNoSlip.sqrMagnitude < 0.01f) vNoSlip = vWorld;

        Vector3 airflow = -vNoSlip.normalized;

        // Lift direction (correct cross order)
        Vector3 liftVec = Vector3.Cross(transform.right, airflow);
        if (liftVec.sqrMagnitude > 1e-6f)
        {
            Vector3 liftDir = liftVec.normalized;
            float lift = q * wingArea * cl * stallFactor * forwardness;
            rb.AddForce(liftDir * lift, ForceMode.Force);
        }

        // Drag (parasitic + induced + flaps drag)
        float cd = (cd0 + flaps * flapsDragBonus) + inducedDrag * cl * cl;
        float drag = q * wingArea * cd;
        rb.AddForce(-velDir * drag, ForceMode.Force);

        // Side force: kills sideways skid
        float side = q * wingArea * (-beta * sideForce) * forwardness;
        rb.AddForce(transform.right * side, ForceMode.Force);

        // Damping: stops tumbling
        float damp = angularDamping + speed * speedAngularDamping;
        rb.AddTorque(-rb.angularVelocity * damp, ForceMode.Force);

        // Extra yaw stability from sideslip
        rb.AddTorque(transform.up * (-beta * yawStability * q * 0.001f), ForceMode.Force);
    }
}

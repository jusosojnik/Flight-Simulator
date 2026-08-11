// using UnityEngine;

// public class AeroSurface : MonoBehaviour
// {
//     [Header("Debug")]
//     public bool debugSurface = false;
//     public float logIntervalSeconds = 0.5f;
//     float _nextLogTime;

//     [Header("References")]
//     public Rigidbody aircraftRb;

//     [Header("Surface Setup")]
//     public float area = 5f;
//     public Vector3 spanAxis = Vector3.right;

//     [Header("Surface Type")]
//     public bool symmetricAirfoil = false;

//     [Header("Lift Model")]
//     public float liftSlope = 5.0f;
//     public float baseLift = 0.1f;
//     public float maxLift = 1.6f;

//     [Header("Drag Model")]
//     public float baseDrag = 0.02f;
//     public float inducedDrag = 0.08f;

//     [Header("Control Effect")]
//     public bool affectedByRoll;
//     public bool affectedByPitch;
//     public bool affectedByYaw;
//     public float controlPower = 0.1f;

//     [Header("Control Surface Limits")]
//     public float maxElevatorDeflectionDeg = 25f;

//     AircraftControls controls;

//     void Start()
//     {
//         if (!aircraftRb)
//         {
//             Debug.LogError($"{name}: aircraftRb not assigned.");
//             enabled = false;
//             return;
//         }

//         controls = aircraftRb.GetComponent<AircraftControls>();
//     }

//     void FixedUpdate()
//     {
//         ApplyAerodynamics();

//         if (debugSurface)
//         {
//             Debug.DrawRay(transform.position, transform.up * 3f, Color.green);
//             Debug.DrawRay(transform.position, transform.right * 3f, Color.red);
//             Debug.DrawRay(transform.position, transform.forward * 3f, Color.blue);
//         }
//     }

//     void ApplyAerodynamics()
//     {
//         Vector3 worldVelocity = aircraftRb.GetPointVelocity(transform.position);
//         Vector3 airflow = -worldVelocity;

//         float speed = airflow.magnitude;
//         if (speed < 0.1f) return;

//         Vector3 airflowDir = airflow / speed;
//         Vector3 localVel = transform.InverseTransformDirection(worldVelocity);

//         float rho = 1.225f;
//         float dynamicPressure = 0.5f * rho * speed * speed;

//         bool isFin = affectedByYaw && !affectedByPitch && !affectedByRoll;

//         float alphaRad = 0f;
//         float betaRad = 0f;
//         float aeroAngleRad;

//         if (isFin)
//         {
//             // sideslip beta (yaw plane). Use LOCAL VELOCITY.
//             Vector3 horizontalVel = new Vector3(localVel.x, 0f, localVel.z);
//             if (horizontalVel.sqrMagnitude < 0.0001f) return;

//             betaRad = Mathf.Atan2(horizontalVel.x, horizontalVel.z);
//             aeroAngleRad = betaRad;
//         }
//         else
//         {
//             alphaRad = Mathf.Atan2(-localVel.y, localVel.z);

//             if (affectedByPitch && controls != null)
//             {
//                 float elevatorDeflectionRad =
//                     controls.pitch * maxElevatorDeflectionDeg * Mathf.Deg2Rad;

//                 alphaRad += elevatorDeflectionRad;
//             }

//             aeroAngleRad = alphaRad;
//         }

//         // --- Stable geometric left/right detection ---
//         if (controls != null)
//         {
//             if (affectedByRoll)
//             {
//                 Vector3 r = transform.position - aircraftRb.worldCenterOfMass;
//                 float side = Mathf.Sign(Vector3.Dot(r, aircraftRb.transform.right));

//                 if (Mathf.Abs(Vector3.Dot(r, aircraftRb.transform.right)) < 0.0001f)
//                     side = 0f;

//                 aeroAngleRad += controls.roll * controlPower * -side;
//             }

//             if (affectedByYaw)
//                 aeroAngleRad += controls.yaw * controlPower;
//         }

//         // --- Stall Model ---
//         float stallRad = 18f * Mathf.Deg2Rad;

//         float camberLift = symmetricAirfoil ? 0f : baseLift;
//         float geometricCL = camberLift + liftSlope * aeroAngleRad;

//         float wingStall = 1f / (1f + Mathf.Pow(aeroAngleRad / stallRad, 4f));
//         float controlStall = 1f / (1f + Mathf.Pow(aeroAngleRad / (stallRad * 1.8f), 2f));

//         float CL = geometricCL * wingStall;
//         CL += liftSlope * (aeroAngleRad - aeroAngleRad * wingStall) * controlStall;
//         CL = Mathf.Clamp(CL, -maxLift, maxLift);

//         float CD = baseDrag + inducedDrag * CL * CL;

//         float liftForce = dynamicPressure * area * CL;
//         float dragForce = dynamicPressure * area * CD;

//         Vector3 totalForce;
//         Vector3 liftDir = Vector3.zero;

//         if (isFin)
//         {
//             Vector3 horizontalVel = new Vector3(localVel.x, 0f, localVel.z);
//             if (horizontalVel.sqrMagnitude < 0.0001f) return;

//             // float beta = Mathf.Atan2(horizontalVel.x, horizontalVel.z);



//             // float CY = liftSlope * beta;

//             float CY = liftSlope * aeroAngleRad; // beta + rudder/yaw control
//             CY = Mathf.Clamp(CY, -maxLift, maxLift);

//             float sideForce = dynamicPressure * area * CY;

//             Vector3 sideDir = aircraftRb.transform.right;
//             Vector3 sideForceWorld = -sideDir * sideForce;

//             Vector3 dragForceWorld = airflowDir * (dynamicPressure * area * baseDrag);

//             totalForce = sideForceWorld + dragForceWorld;

//             liftDir = Vector3.zero; // fin has no lift direction
//         }
//         else
//         {
//             Vector3 spanWorld = transform.TransformDirection(spanAxis).normalized;
//             Vector3 liftDirRaw = Vector3.Cross(spanWorld, airflowDir);

//             if (affectedByPitch && !affectedByYaw)
//             {
//                 Vector3 aircraftRight = aircraftRb.transform.right;
//                 liftDirRaw = Vector3.ProjectOnPlane(liftDirRaw, aircraftRight);
//             }

//             if (liftDirRaw.sqrMagnitude > 0.0001f)
//                 liftDir = liftDirRaw.normalized;

//             totalForce = liftDir * liftForce + airflowDir * dragForce;
//         }

//         // --- NUMERICAL STABILITY LIMITER ---
//         // Prevent forces that cannot be integrated in one physics step.

//         // float maxAccel = 50f; // m/s² (~5g, already aggressive)
//         // float maxForce = aircraftRb.mass * maxAccel;

//         // if (totalForce.magnitude > maxForce)
//         // {
//         //     totalForce = totalForce.normalized * maxForce;
//         // }

//         float maxAccel = 50f; // m/s² (~5g, already aggressive)
//         float maxForce = aircraftRb.mass * maxAccel;

//         if (totalForce.magnitude > maxForce && speed > 300)
//         {
//             totalForce = totalForce.normalized * maxForce;
//         }

//         aircraftRb.AddForceAtPosition(totalForce, transform.position);

//         if (debugSurface)
//         {
//             Debug.DrawLine(transform.position, transform.position + airflowDir * 5f, Color.cyan);
//             Debug.DrawLine(transform.position, transform.position + totalForce * 0.0005f, Color.yellow);
//             Debug.DrawLine(transform.position, transform.position + liftDir * 2f, Color.magenta);
//         }

//         LogThrottled(worldVelocity, localVel, alphaRad, betaRad, aeroAngleRad, CL, liftDir, totalForce);
//     }

//     void LogThrottled(
//         Vector3 worldVel,
//         Vector3 localVel,
//         float alphaRad,
//         float betaRad,
//         float aeroAngleRad,
//         float CL,
//         Vector3 liftDir,
//         Vector3 totalForce)
//     {
//         if (!debugSurface) return;
//         if (logIntervalSeconds > 0f && Time.time < _nextLogTime) return;

//         _nextLogTime = Time.time + Mathf.Max(0.01f, logIntervalSeconds);

//         Debug.Log(
//             $"[{name}] Speed:{worldVel.magnitude:F1} " +
//             $"AoA:{alphaRad * Mathf.Rad2Deg:F2}° " +
//             $"Beta:{betaRad * Mathf.Rad2Deg:F2}° " +
//             $"CL:{CL:F3} Force:{totalForce}");
//     }
// }



using UnityEngine;

public class AeroSurface : MonoBehaviour
{
    [Header("Debug")]
    public bool debugSurface = false;
    public float logIntervalSeconds = 0.5f;
    float _nextLogTime;

    [Header("References")]
    public Rigidbody aircraftRb;

    [Header("Surface Setup")]
    public float area = 5f;
    public Vector3 spanAxis = Vector3.right;

    [Header("Surface Type")]
    public bool symmetricAirfoil = false;

    [Header("Lift Model")]
    public float liftSlope = 5.0f;
    public float baseLift = 0.1f;
    public float maxLift = 1.6f;

    [Header("Drag Model")]
    public float baseDrag = 0.02f;
    public float inducedDrag = 0.08f;

    [Header("Control Effect")]
    public bool affectedByRoll;
    public bool affectedByPitch;
    public bool affectedByYaw;
    public float controlPower = 0.1f;

    [Header("Control Surface Limits")]
    public float maxElevatorDeflectionDeg = 25f;

    [Header("High-Speed Aero Assist (Gameplay)")]
    public bool useHighSpeedAeroAssist = true;
    [Tooltip("Below this speed, aerodynamics use real speed.")]
    public float aeroLinearSpeed = 250f;
    [Tooltip("Effective aero speed asymptotically approaches this value.")]
    public float aeroMaxEffectiveSpeed = 350f;
    [Tooltip("Higher = stronger compression above aeroLinearSpeed.")]
    public float aeroCompressionStrength = 0.01f;
    [Tooltip("Optional hard cap after compression.")]
    public bool hardClampAeroSpeed = false;
    public float hardMaxAeroSpeed = 300f;

    [Header("Per-Axis High-Speed Control Damping")]
    public bool dampPitchAtHighSpeed = false; // keep pitch responsive by default
    public float pitchDampingStartSpeed = 280f;
    public float pitchDampingEndSpeed = 500f;
    [Range(0f, 1f)] public float minPitchAuthority = 0.7f;

    public bool dampRollYawAtHighSpeed = true;
    public float rollYawDampingStartSpeed = 180f;
    public float rollYawDampingEndSpeed = 350f;
    [Range(0f, 1f)] public float minRollYawAuthority = 0.25f;

    [Header("Force Safety Limit")]
    public bool limitExtremeForce = true;
    [Tooltip("Max acceleration-equivalent force per surface.")]
    public float maxAccelPerSurface = 50f;
    [Tooltip("Pitch surfaces can get extra force budget to preserve pull-up/down authority.")]
    public float pitchSurfaceForceMultiplier = 2.5f;
    [Tooltip("Only clamp if actual speed exceeds this.")]
    public float forceClampStartSpeed = 300f;

    [Header("Optional Stability Helpers")]
    public bool setRigidBodyMaxAngularVelocityOnStart = false;
    public float rigidbodyMaxAngularVelocity = 10f;

    AircraftControls controls;

    void Start()
    {
        if (!aircraftRb)
        {
            Debug.LogError($"{name}: aircraftRb not assigned.");
            enabled = false;
            return;
        }

        controls = aircraftRb.GetComponent<AircraftControls>();

        if (setRigidBodyMaxAngularVelocityOnStart)
        {
            aircraftRb.maxAngularVelocity = rigidbodyMaxAngularVelocity;
        }
    }

    void FixedUpdate()
    {
        ApplyAerodynamics();

        if (debugSurface)
        {
            Debug.DrawRay(transform.position, transform.up * 3f, Color.green);
            Debug.DrawRay(transform.position, transform.right * 3f, Color.red);
            Debug.DrawRay(transform.position, transform.forward * 3f, Color.blue);
        }
    }

    float GetEffectiveAeroSpeed(float actualSpeed)
    {
        if (!useHighSpeedAeroAssist)
            return actualSpeed;

        if (actualSpeed <= aeroLinearSpeed)
        {
            float s0 = Mathf.Max(0f, actualSpeed);
            if (hardClampAeroSpeed) s0 = Mathf.Min(s0, hardMaxAeroSpeed);
            return s0;
        }

        float room = Mathf.Max(0.001f, aeroMaxEffectiveSpeed - aeroLinearSpeed);
        float excess = actualSpeed - aeroLinearSpeed;

        // Soft saturation curve
        float compressed = aeroLinearSpeed + room * (1f - Mathf.Exp(-aeroCompressionStrength * excess));

        if (hardClampAeroSpeed)
            compressed = Mathf.Min(compressed, hardMaxAeroSpeed);

        return Mathf.Max(0f, compressed);
    }

    float GetAuthorityFactor(float actualSpeed, bool enabled, float start, float end, float minAuthority)
    {
        if (!enabled) return 1f;

        if (actualSpeed <= start) return 1f;
        if (actualSpeed >= end) return minAuthority;

        float t = Mathf.InverseLerp(start, end, actualSpeed);
        return Mathf.Lerp(1f, minAuthority, t);
    }

    void ApplyAerodynamics()
    {
        Vector3 worldVelocity = aircraftRb.GetPointVelocity(transform.position);
        Vector3 airflow = -worldVelocity;

        float speed = airflow.magnitude;
        if (speed < 0.1f) return;

        Vector3 airflowDir = airflow / speed;
        Vector3 localVel = transform.InverseTransformDirection(worldVelocity);

        // Air density (sea-level approximation)
        float rho = 1.225f;

        // Gameplay: compress aerodynamic response at very high speed
        float effectiveAeroSpeed = GetEffectiveAeroSpeed(speed);
        float dynamicPressure = 0.5f * rho * effectiveAeroSpeed * effectiveAeroSpeed;

        // Separate control damping so pitch doesn't die at high speed
        float pitchAuthority = GetAuthorityFactor(
            speed,
            dampPitchAtHighSpeed,
            pitchDampingStartSpeed,
            pitchDampingEndSpeed,
            minPitchAuthority
        );

        float rollYawAuthority = GetAuthorityFactor(
            speed,
            dampRollYawAtHighSpeed,
            rollYawDampingStartSpeed,
            rollYawDampingEndSpeed,
            minRollYawAuthority
        );

        bool isFin = affectedByYaw && !affectedByPitch && !affectedByRoll;

        float alphaRad = 0f;
        float betaRad = 0f;
        float aeroAngleRad;

        if (isFin)
        {
            // Sideslip beta in local XZ plane
            Vector3 horizontalVel = new Vector3(localVel.x, 0f, localVel.z);
            if (horizontalVel.sqrMagnitude < 0.0001f) return;

            betaRad = Mathf.Atan2(horizontalVel.x, horizontalVel.z);
            aeroAngleRad = betaRad;
        }
        else
        {
            // AoA in local YZ plane
            alphaRad = Mathf.Atan2(-localVel.y, localVel.z);

            // Elevator deflection (pitch) - intentionally less damped
            if (affectedByPitch && controls != null)
            {
                float elevatorDeflectionRad =
                    controls.pitch * maxElevatorDeflectionDeg * Mathf.Deg2Rad * pitchAuthority;

                alphaRad += elevatorDeflectionRad;
            }

            aeroAngleRad = alphaRad;
        }

        // Control effects for roll/yaw
        if (controls != null)
        {
            if (affectedByRoll)
            {
                Vector3 r = transform.position - aircraftRb.worldCenterOfMass;
                float sideDot = Vector3.Dot(r, aircraftRb.transform.right);
                float side = Mathf.Sign(sideDot);

                if (Mathf.Abs(sideDot) < 0.0001f)
                    side = 0f;

                aeroAngleRad += controls.roll * controlPower * rollYawAuthority * -side;
            }

            if (affectedByYaw)
            {
                aeroAngleRad += controls.yaw * controlPower * rollYawAuthority;
            }
        }

        // --- Stall Model ---
        float stallRad = 18f * Mathf.Deg2Rad;

        float camberLift = symmetricAirfoil ? 0f : baseLift;
        float geometricCL = camberLift + liftSlope * aeroAngleRad;

        float wingStall = 1f / (1f + Mathf.Pow(aeroAngleRad / stallRad, 4f));
        float controlStall = 1f / (1f + Mathf.Pow(aeroAngleRad / (stallRad * 1.8f), 2f));

        float CL = geometricCL * wingStall;
        CL += liftSlope * (aeroAngleRad - aeroAngleRad * wingStall) * controlStall;
        CL = Mathf.Clamp(CL, -maxLift, maxLift);

        float CD = baseDrag + inducedDrag * CL * CL;

        float liftForce = dynamicPressure * area * CL;
        float dragForce = dynamicPressure * area * CD;

        Vector3 totalForce;
        Vector3 liftDir = Vector3.zero;

        if (isFin)
        {
            Vector3 horizontalVel = new Vector3(localVel.x, 0f, localVel.z);
            if (horizontalVel.sqrMagnitude < 0.0001f) return;

            float CY = liftSlope * aeroAngleRad; // beta + yaw/rudder control
            CY = Mathf.Clamp(CY, -maxLift, maxLift);

            float sideForce = dynamicPressure * area * CY;

            // Flip sign if rudder response feels inverted
            Vector3 sideDir = aircraftRb.transform.right;
            Vector3 sideForceWorld = -sideDir * sideForce;

            Vector3 dragForceWorld = airflowDir * (dynamicPressure * area * baseDrag);

            totalForce = sideForceWorld + dragForceWorld;
            liftDir = Vector3.zero;
        }
        else
        {
            Vector3 spanWorld = transform.TransformDirection(spanAxis).normalized;
            Vector3 liftDirRaw = Vector3.Cross(spanWorld, airflowDir);

            // Optional stabilization for pitch-controlled surfaces
            if (affectedByPitch && !affectedByYaw)
            {
                Vector3 aircraftRight = aircraftRb.transform.right;
                liftDirRaw = Vector3.ProjectOnPlane(liftDirRaw, aircraftRight);
            }

            if (liftDirRaw.sqrMagnitude > 0.0001f)
                liftDir = liftDirRaw.normalized;

            // airflowDir points opposite velocity, so this drag resists motion
            totalForce = liftDir * liftForce + airflowDir * dragForce;
        }

        // --- Numerical Stability Force Clamp ---
        if (limitExtremeForce && speed > forceClampStartSpeed)
        {
            float surfaceAccelLimit = maxAccelPerSurface;

            // Preserve pitch authority by giving pitch surfaces more force budget
            if (affectedByPitch)
                surfaceAccelLimit *= Mathf.Max(1f, pitchSurfaceForceMultiplier);

            float maxForce = aircraftRb.mass * surfaceAccelLimit;

            if (totalForce.magnitude > maxForce)
                totalForce = totalForce.normalized * maxForce;
        }

        aircraftRb.AddForceAtPosition(totalForce, transform.position);

        if (debugSurface)
        {
            Debug.DrawLine(transform.position, transform.position + airflowDir * 5f, Color.cyan);
            Debug.DrawLine(transform.position, transform.position + totalForce * 0.0005f, Color.yellow);
            Debug.DrawLine(transform.position, transform.position + liftDir * 2f, Color.magenta);
        }

        LogThrottled(
            actualSpeed: speed,
            effectiveAeroSpeed: effectiveAeroSpeed,
            alphaRad: alphaRad,
            betaRad: betaRad,
            CL: CL,
            totalForce: totalForce,
            pitchAuthority: pitchAuthority,
            rollYawAuthority: rollYawAuthority
        );
    }

    void LogThrottled(
        float actualSpeed,
        float effectiveAeroSpeed,
        float alphaRad,
        float betaRad,
        float CL,
        Vector3 totalForce,
        float pitchAuthority,
        float rollYawAuthority)
    {
        if (!debugSurface) return;
        if (logIntervalSeconds > 0f && Time.time < _nextLogTime) return;

        _nextLogTime = Time.time + Mathf.Max(0.01f, logIntervalSeconds);

        Debug.Log(
            $"[{name}] " +
            $"Speed:{actualSpeed:F1} " +
            $"AeroSpeed:{effectiveAeroSpeed:F1} " +
            $"PitchAuth:{pitchAuthority:F2} " +
            $"RollYawAuth:{rollYawAuthority:F2} " +
            $"AoA:{alphaRad * Mathf.Rad2Deg:F2}° " +
            $"Beta:{betaRad * Mathf.Rad2Deg:F2}° " +
            $"CL:{CL:F3} " +
            $"Force:{totalForce.magnitude:F1}"
        );
    }
}


// using UnityEngine;

// public class AeroSurface : MonoBehaviour
// {
//     [Header("Debug")]
//     public bool debugSurface = false;
//     public float logIntervalSeconds = 0.5f;
//     float _nextLogTime;

//     [Header("References")]
//     public Rigidbody aircraftRb;

//     [Header("Surface Setup")]
//     public float area = 5f;
//     public Vector3 spanAxis = Vector3.right;

//     [Header("Surface Type")]
//     public bool symmetricAirfoil = false;

//     [Header("Lift Model")]
//     public float liftSlope = 5.0f;
//     public float baseLift = 0.1f;
//     public float maxLift = 1.6f;

//     [Header("Drag Model")]
//     public float baseDrag = 0.02f;
//     public float inducedDrag = 0.08f;

//     [Header("Control Effect")]
//     public bool affectedByRoll;
//     public bool affectedByPitch;
//     public bool affectedByYaw;
//     public float controlPower = 0.1f;

//     [Header("Control Surface Limits")]
//     public float maxElevatorDeflectionDeg = 25f;

//     [Header("High-Speed Aero Assist (Gameplay)")]
//     public bool useHighSpeedAeroAssist = true;

//     [Tooltip("Below this speed, aerodynamics use real speed (linear/normal region).")]
//     public float aeroLinearSpeed = 250f;

//     [Tooltip("Effective aero speed will asymptotically approach this value.")]
//     public float aeroMaxEffectiveSpeed = 350f;

//     [Tooltip("How quickly aero speed compresses above aeroLinearSpeed. Higher = stronger compression.")]
//     public float aeroCompressionStrength = 0.01f;

//     [Tooltip("If enabled, effective aero speed is hard-clamped after compression.")]
//     public bool hardClampAeroSpeed = false;

//     [Tooltip("Hard cap for effective aero speed (used if hardClampAeroSpeed is enabled).")]
//     public float hardMaxAeroSpeed = 300f;

//     [Header("High-Speed Control Damping (Gameplay)")]
//     public bool dampControlsAtHighSpeed = true;
//     public float controlDampingStartSpeed = 200f;
//     public float controlDampingEndSpeed = 400f;
//     [Range(0f, 1f)] public float minControlAuthority = 0.25f;

//     [Header("Force Safety Limit")]
//     public bool limitExtremeForce = true;
//     [Tooltip("Max acceleration-equivalent force per surface at extreme speeds.")]
//     public float maxAccelPerSurface = 50f;
//     [Tooltip("Only clamp if speed exceeds this (so normal low-speed behavior remains untouched).")]
//     public float forceClampStartSpeed = 300f;

//     [Header("Optional Stability Helpers")]
//     public bool setRigidBodyMaxAngularVelocityOnStart = false;
//     public float rigidbodyMaxAngularVelocity = 10f;

//     AircraftControls controls;

//     void Start()
//     {
//         if (!aircraftRb)
//         {
//             Debug.LogError($"{name}: aircraftRb not assigned.");
//             enabled = false;
//             return;
//         }

//         controls = aircraftRb.GetComponent<AircraftControls>();

//         if (setRigidBodyMaxAngularVelocityOnStart)
//         {
//             aircraftRb.maxAngularVelocity = rigidbodyMaxAngularVelocity;
//         }
//     }

//     void FixedUpdate()
//     {
//         ApplyAerodynamics();

//         if (debugSurface)
//         {
//             Debug.DrawRay(transform.position, transform.up * 3f, Color.green);
//             Debug.DrawRay(transform.position, transform.right * 3f, Color.red);
//             Debug.DrawRay(transform.position, transform.forward * 3f, Color.blue);
//         }
//     }

//     float GetEffectiveAeroSpeed(float actualSpeed)
//     {
//         if (!useHighSpeedAeroAssist)
//             return actualSpeed;

//         // Normal behavior below threshold
//         if (actualSpeed <= aeroLinearSpeed)
//         {
//             float s0 = Mathf.Max(0f, actualSpeed);
//             if (hardClampAeroSpeed) s0 = Mathf.Min(s0, hardMaxAeroSpeed);
//             return s0;
//         }

//         // Soft compression above threshold
//         float room = Mathf.Max(0.001f, aeroMaxEffectiveSpeed - aeroLinearSpeed);
//         float excess = actualSpeed - aeroLinearSpeed;

//         float compressed = aeroLinearSpeed + room * (1f - Mathf.Exp(-aeroCompressionStrength * excess));

//         if (hardClampAeroSpeed)
//             compressed = Mathf.Min(compressed, hardMaxAeroSpeed);

//         return Mathf.Max(0f, compressed);
//     }

//     float GetControlAuthorityFactor(float actualSpeed)
//     {
//         if (!dampControlsAtHighSpeed)
//             return 1f;

//         if (actualSpeed <= controlDampingStartSpeed)
//             return 1f;

//         if (actualSpeed >= controlDampingEndSpeed)
//             return minControlAuthority;

//         float t = Mathf.InverseLerp(controlDampingStartSpeed, controlDampingEndSpeed, actualSpeed);
//         return Mathf.Lerp(1f, minControlAuthority, t);
//     }

//     void ApplyAerodynamics()
//     {
//         Vector3 worldVelocity = aircraftRb.GetPointVelocity(transform.position);
//         Vector3 airflow = -worldVelocity;

//         float speed = airflow.magnitude;
//         if (speed < 0.1f) return;

//         Vector3 airflowDir = airflow / speed;
//         Vector3 localVel = transform.InverseTransformDirection(worldVelocity);

//         // Air density (sea-level approximation; can be replaced by an atmosphere model later)
//         float rho = 1.225f;

//         // Gameplay: decouple aerodynamic force growth from actual speed
//         float effectiveAeroSpeed = GetEffectiveAeroSpeed(speed);
//         float dynamicPressure = 0.5f * rho * effectiveAeroSpeed * effectiveAeroSpeed;

//         // Optional high-speed control damping
//         float controlAuthority = GetControlAuthorityFactor(speed);

//         bool isFin = affectedByYaw && !affectedByPitch && !affectedByRoll;

//         float alphaRad = 0f;
//         float betaRad = 0f;
//         float aeroAngleRad;

//         if (isFin)
//         {
//             // Sideslip beta in the local XZ plane
//             Vector3 horizontalVel = new Vector3(localVel.x, 0f, localVel.z);
//             if (horizontalVel.sqrMagnitude < 0.0001f) return;

//             betaRad = Mathf.Atan2(horizontalVel.x, horizontalVel.z);
//             aeroAngleRad = betaRad;
//         }
//         else
//         {
//             // Angle of attack in local YZ plane
//             alphaRad = Mathf.Atan2(-localVel.y, localVel.z);

//             if (affectedByPitch && controls != null)
//             {
//                 float elevatorDeflectionRad =
//                     controls.pitch * maxElevatorDeflectionDeg * Mathf.Deg2Rad * controlAuthority;

//                 alphaRad += elevatorDeflectionRad;
//             }

//             aeroAngleRad = alphaRad;
//         }

//         // Stable geometric left/right detection for roll surfaces
//         if (controls != null)
//         {
//             if (affectedByRoll)
//             {
//                 Vector3 r = transform.position - aircraftRb.worldCenterOfMass;
//                 float sideDot = Vector3.Dot(r, aircraftRb.transform.right);
//                 float side = Mathf.Sign(sideDot);

//                 if (Mathf.Abs(sideDot) < 0.0001f)
//                     side = 0f;

//                 aeroAngleRad += controls.roll * controlPower * controlAuthority * -side;
//             }

//             if (affectedByYaw)
//             {
//                 aeroAngleRad += controls.yaw * controlPower * controlAuthority;
//             }
//         }

//         // --- Stall Model ---
//         float stallRad = 18f * Mathf.Deg2Rad;

//         float camberLift = symmetricAirfoil ? 0f : baseLift;
//         float geometricCL = camberLift + liftSlope * aeroAngleRad;

//         float wingStall = 1f / (1f + Mathf.Pow(aeroAngleRad / stallRad, 4f));
//         float controlStall = 1f / (1f + Mathf.Pow(aeroAngleRad / (stallRad * 1.8f), 2f));

//         float CL = geometricCL * wingStall;
//         CL += liftSlope * (aeroAngleRad - aeroAngleRad * wingStall) * controlStall;
//         CL = Mathf.Clamp(CL, -maxLift, maxLift);

//         float CD = baseDrag + inducedDrag * CL * CL;

//         float liftForce = dynamicPressure * area * CL;
//         float dragForce = dynamicPressure * area * CD;

//         Vector3 totalForce;
//         Vector3 liftDir = Vector3.zero;

//         if (isFin)
//         {
//             Vector3 horizontalVel = new Vector3(localVel.x, 0f, localVel.z);
//             if (horizontalVel.sqrMagnitude < 0.0001f) return;

//             float CY = liftSlope * aeroAngleRad; // beta + rudder/yaw control
//             CY = Mathf.Clamp(CY, -maxLift, maxLift);

//             float sideForce = dynamicPressure * area * CY;

//             // Fin side force direction (flip sign here if rudder feels inverted)
//             Vector3 sideDir = aircraftRb.transform.right;
//             Vector3 sideForceWorld = -sideDir * sideForce;

//             // Use compressed aero pressure for drag too (gameplay consistency)
//             Vector3 dragForceWorld = airflowDir * (dynamicPressure * area * baseDrag);

//             totalForce = sideForceWorld + dragForceWorld;

//             liftDir = Vector3.zero; // fin modeled as sideforce + drag
//         }
//         else
//         {
//             Vector3 spanWorld = transform.TransformDirection(spanAxis).normalized;
//             Vector3 liftDirRaw = Vector3.Cross(spanWorld, airflowDir);

//             // Optional stabilization for pitch-controlled surfaces
//             if (affectedByPitch && !affectedByYaw)
//             {
//                 Vector3 aircraftRight = aircraftRb.transform.right;
//                 liftDirRaw = Vector3.ProjectOnPlane(liftDirRaw, aircraftRight);
//             }

//             if (liftDirRaw.sqrMagnitude > 0.0001f)
//                 liftDir = liftDirRaw.normalized;

//             // airflowDir points opposite velocity, so adding drag along airflowDir resists motion
//             totalForce = liftDir * liftForce + airflowDir * dragForce;
//         }

//         // --- Numerical Stability Force Clamp (only at extreme speed) ---
//         if (limitExtremeForce && speed > forceClampStartSpeed)
//         {
//             float maxForce = aircraftRb.mass * maxAccelPerSurface;
//             if (totalForce.magnitude > maxForce)
//             {
//                 totalForce = totalForce.normalized * maxForce;
//             }
//         }

//         aircraftRb.AddForceAtPosition(totalForce, transform.position);

//         if (debugSurface)
//         {
//             Debug.DrawLine(transform.position, transform.position + airflowDir * 5f, Color.cyan);
//             Debug.DrawLine(transform.position, transform.position + totalForce * 0.0005f, Color.yellow);
//             Debug.DrawLine(transform.position, transform.position + liftDir * 2f, Color.magenta);
//         }

//         LogThrottled(
//             worldVelocity,
//             localVel,
//             speed,
//             effectiveAeroSpeed,
//             alphaRad,
//             betaRad,
//             aeroAngleRad,
//             CL,
//             liftDir,
//             totalForce,
//             controlAuthority
//         );
//     }

//     void LogThrottled(
//         Vector3 worldVel,
//         Vector3 localVel,
//         float actualSpeed,
//         float effectiveAeroSpeed,
//         float alphaRad,
//         float betaRad,
//         float aeroAngleRad,
//         float CL,
//         Vector3 liftDir,
//         Vector3 totalForce,
//         float controlAuthority)
//     {
//         if (!debugSurface) return;
//         if (logIntervalSeconds > 0f && Time.time < _nextLogTime) return;

//         _nextLogTime = Time.time + Mathf.Max(0.01f, logIntervalSeconds);

//         Debug.Log(
//             $"[{name}] " +
//             $"Speed:{actualSpeed:F1} " +
//             $"AeroSpeed:{effectiveAeroSpeed:F1} " +
//             $"CtrlAuth:{controlAuthority:F2} " +
//             $"AoA:{alphaRad * Mathf.Rad2Deg:F2}° " +
//             $"Beta:{betaRad * Mathf.Rad2Deg:F2}° " +
//             $"CL:{CL:F3} " +
//             $"Force:{totalForce}");
//     }
// }
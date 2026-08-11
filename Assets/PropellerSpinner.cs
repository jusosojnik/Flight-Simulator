using UnityEngine;

public class PropellerSpinner : MonoBehaviour
{
    [Header("Engine Reference")]
    public AircraftEngine engine;   // drag Aircraft here

    [Header("Spin Settings")]
    public float maxRPM = 2400f;
    public float spoolSpeed = 4f;   // how fast it accelerates

    float currentRPM;

    void Update()
    {
        if (engine == null) return;

        // Use the REAL throttle already calculated by AircraftEngine
        float targetRPM = engine.throttle * maxRPM;

        // Smooth engine spool-up
        currentRPM = Mathf.Lerp(currentRPM, targetRPM, Time.deltaTime * spoolSpeed);

        // Convert RPM to degrees/sec
        float degreesPerSecond = currentRPM * 6f;

        // Spin (change axis if needed)
        transform.Rotate(Vector3.forward, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}

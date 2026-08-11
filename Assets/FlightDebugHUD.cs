using UnityEngine;
using TMPro;

public class FlightDebugHUD : MonoBehaviour
{
    public Rigidbody aircraftRb;
    public AircraftEngine engine;

    public AircraftControls controls;


    public TextMeshProUGUI text;

    void Update()
    {
        if (aircraftRb == null) return;

        float speed = aircraftRb.linearVelocity.magnitude;
        float altitude = aircraftRb.position.y;

        Vector3 localVel = aircraftRb.transform.InverseTransformDirection(aircraftRb.linearVelocity);

        text.text =
             "=== INPUT ===\n" +
             "Throttle Input: " + engine.throttleInput.ToString("0.00") + "\n" +
             "Pitch Input:    " + controls.pitch.ToString("0.00") + "\n" +
             "Roll Input:     " + controls.roll.ToString("0.00") + "\n" +
             "Yaw Input:      " + controls.yaw.ToString("0.00") + "\n\n" +

             "=== STATE ===\n" +
             "Throttle: " + engine.throttle.ToString("0.00") + "\n" +
             "Speed: " + (aircraftRb.linearVelocity.magnitude * 3.6).ToString("0.0") + " km/h\n" +
             "Altitude: " + aircraftRb.position.y.ToString("0.0") + " m\n";

    }


}

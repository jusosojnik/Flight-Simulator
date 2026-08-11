using UnityEngine;
using UnityEngine.InputSystem;

public class AircraftControls : MonoBehaviour
{
    InputSystem_Actions controls;

    // Raw pilot input (-1 to 1)
    float pitchInput;
    float rollInput;
    float yawInput;

    [Header("Control Response")]
    public float pitchSpeed = 3f;   // how fast surfaces move
    public float rollSpeed = 4f;
    public float yawSpeed = 2f;

    [Header("Current Control State (read by AeroSurface)")]
    public float pitch;
    public float roll;
    public float yaw;

    void OnEnable()
    {
        if (controls == null)
        {
            controls = new InputSystem_Actions();

            controls.Aircraft.Pitch.performed += ctx => pitchInput = ctx.ReadValue<float>();
            controls.Aircraft.Pitch.canceled += ctx => pitchInput = 0f;

            controls.Aircraft.Roll.performed += ctx => rollInput = ctx.ReadValue<float>();
            controls.Aircraft.Roll.canceled += ctx => rollInput = 0f;

            controls.Aircraft.Yaw.performed += ctx => yawInput = ctx.ReadValue<float>();
            controls.Aircraft.Yaw.canceled += ctx => yawInput = 0f;
        }

        controls.Enable();
    }

    void OnDisable() => controls.Disable();

    void Update()
    {
        // Smoothly move toward pilot command
        pitch = Mathf.MoveTowards(pitch, pitchInput, pitchSpeed * Time.deltaTime);
        roll = Mathf.MoveTowards(roll, rollInput, rollSpeed * Time.deltaTime);
        yaw = Mathf.MoveTowards(yaw, yawInput, yawSpeed * Time.deltaTime);
    }
}

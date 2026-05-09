using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceController : MonoBehaviour
{
    [Header("Movement")]
    public float thrustStrength = 20f;

    [Header("Rotation")]
    public float rotSpeed = 3f;
    public float rollSpeed = 100f;
    public float rotSmoothSpeed = 5f;

    [Header("Keybinds")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode ascendKey = KeyCode.Space;
    public KeyCode descendKey = KeyCode.LeftControl;
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode rollClockwiseKey = KeyCode.E;
    public KeyCode rollCounterKey = KeyCode.Q;

    public KeyCode toggleMouseLockKey = KeyCode.Escape;
    bool isMouseLocked = true;

    private Rigidbody rb;

    private Vector3 thrusterInput;
    private Quaternion targetRot;
    private Quaternion smoothedRot;

    void Start()
    {
        SetMouseLock(true);

        rb = GetComponent<Rigidbody>();
        targetRot = transform.rotation;
    }

    void SetMouseLock(bool locked)
    {
        isMouseLocked = locked;

        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleMouseLockKey))
        {
            SetMouseLock(!isMouseLocked);
        }
        HandleMovement();
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    void HandleMovement()
    {
        // Thruster input
        int thrustInputX = GetInputAxis(leftKey, rightKey);
        int thrustInputY = GetInputAxis(descendKey, ascendKey);
        int thrustInputZ = GetInputAxis(backwardKey, forwardKey);

        thrusterInput = new Vector3(thrustInputX, thrustInputY, thrustInputZ);

        // Rotation input
        float yawInput = Input.GetAxisRaw("Mouse X") * rotSpeed;
        float pitchInput = Input.GetAxisRaw("Mouse Y") * rotSpeed;
        float rollInput = GetInputAxis(rollCounterKey, rollClockwiseKey) * rollSpeed * Time.deltaTime;

        // Build rotations
        Quaternion yaw = Quaternion.AngleAxis(yawInput, transform.up);
        Quaternion pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
        Quaternion roll = Quaternion.AngleAxis(-rollInput, transform.forward);

        targetRot = yaw * pitch * roll * targetRot;

        smoothedRot = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotSmoothSpeed
        );
    }

    void ApplyPhysics()
    {
        // Thrusters
        Vector3 thrustDir = transform.TransformVector(thrusterInput);
        rb.AddForce(thrustDir * thrustStrength, ForceMode.Acceleration);

        // Rotation
        rb.MoveRotation(smoothedRot);
    }

    int GetInputAxis(KeyCode negative, KeyCode positive)
    {
        int value = 0;

        if (Input.GetKey(negative)) value -= 1;
        if (Input.GetKey(positive)) value += 1;

        return value;
    }
}
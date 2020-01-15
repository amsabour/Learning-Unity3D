using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Main Camera")] public Transform cameraTransform;

    [Header("Movement Settings")] public float moveSpeed;
    public float changeDirectionTime;
    public float rotationSpeed;
    public AnimationCurve turnTimeCurve;

    [Header("Jumping Settings")] public float jumpHeight;
    public int numberOfJumps;
    private int jumpsUsed;


    [Header("Gravity Settings")] public float gravity;


    private CharacterController characterController;
    private Vector3 direction = Vector3.zero;
    private Vector3 velocity = Vector3.zero;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    Vector3 ProjectOnXZPlane(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    Vector3 GetInputXZDirection()
    {
        // Compute forward vector
        Vector3 forward = ProjectOnXZPlane(transform.position - cameraTransform.position);
        forward = forward.normalized;

        // Compute right vector
        Vector3 right = Quaternion.AngleAxis(90, Vector3.up) * forward;
        right = right.normalized;


        // Create inputDirection based on input
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        Vector3 inputDirection = v * forward + h * right;
        inputDirection *= moveSpeed;

        return inputDirection;
    }

    float GetTurnTime(Vector3 current, Vector3 target)
    {
        float dot = (Vector3.Dot(current, target) + 1) / 2;
        float coef = turnTimeCurve.Evaluate(dot);
        return coef * changeDirectionTime;
    }

    void UpdateHorizontalDirection()
    {
        // Smooth turning
        Vector3 xzDirection = ProjectOnXZPlane(direction);
        Vector3 inputDirection = GetInputXZDirection();
        float timeToTurn = GetTurnTime(xzDirection, inputDirection);
        xzDirection = Vector3.SmoothDamp(xzDirection, inputDirection, ref velocity, timeToTurn);

        direction.x = xzDirection.x;
        direction.z = xzDirection.z;
    }

    void UpdateVerticalDirection()
    {
        // Apply Gravity
        direction.y -= gravity * Time.deltaTime;

        // Jumping logic
        if (characterController.isGrounded)
        {
            direction.y = -0.1f;
            jumpsUsed = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpsUsed < numberOfJumps)
        {
            direction.y = Mathf.Sqrt(2 * gravity * jumpHeight);
            jumpsUsed += 1;
        }
    }

    void LookForward()
    {
        Vector3 horizontalDirection = ProjectOnXZPlane(direction);

        if (horizontalDirection.magnitude <= 0.1f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHorizontalDirection();
        UpdateVerticalDirection();
        LookForward();
        characterController.Move(direction * Time.deltaTime);
    }


    private void OnDrawGizmos()
    {
        Vector3 forward = transform.position - cameraTransform.position;
        forward.y = 0;
        Vector3 right = Quaternion.AngleAxis(90, Vector3.up) * forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + 4 * forward);
        Gizmos.DrawLine(transform.position, transform.position + 4 * right);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + 6 * direction);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float swimSpeed = 5f;
    [SerializeField] private float swimFastSpeed = 8f;
    [SerializeField] private float rotationSpeed = 500f;
    [SerializeField] private float gravity = 0.2f;

    [Header("References")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference sprintActionReference;

    private Rigidbody2D rb;

    private Vector2 moveInput;

    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        currentSpeed = swimSpeed;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);
        Rotate();
    }

    private void OnEnable()
    {
        moveActionReference.action.Enable();
        moveActionReference.action.performed += OnMovePerformed;
        moveActionReference.action.canceled += OnMoveCancelled;

        sprintActionReference.action.started += Sprint;
        sprintActionReference.action.canceled += SprintStop;
    }
    private void OnDisable()
    {
        moveActionReference.action.performed -= OnMovePerformed;
        moveActionReference.action.canceled -= OnMoveCancelled;
        moveActionReference.action.Disable();

        sprintActionReference.action.started -= Sprint;
        sprintActionReference.action.canceled -= SprintStop;
    }
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        rb.gravityScale = 0;
    }

    private void OnMoveCancelled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
        rb.gravityScale = gravity;
    }

    private void Sprint(InputAction.CallbackContext ctx)
    {
        currentSpeed = swimFastSpeed;
    }

    private void SprintStop(InputAction.CallbackContext ctx)
    {
        currentSpeed = swimSpeed;
    }

    private void Rotate()
    {
        if (moveInput != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, moveInput);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            rb.MoveRotation(rotation);
        }
    }
}

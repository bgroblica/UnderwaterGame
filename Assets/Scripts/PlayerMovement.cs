
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Speed")]
    [SerializeField] private float swimSpeed = 5f;
    [SerializeField] private float swimFastSpeed = 8f;
    [SerializeField] private float swimPenalty = 2f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float gravity = 0.2f;

    [Header("References")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference sprintActionReference;
    [SerializeField] private Camera camera;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentSpeed;
    private bool isDragging = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        currentSpeed = swimSpeed;
    }

    public override void OnNetworkSpawn()
    {
        if (camera != null)
            camera.gameObject.SetActive(IsOwner);

        if (IsOwner)
        {
            moveActionReference.action.Enable();
            sprintActionReference.action.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            moveActionReference.action.Disable();
            sprintActionReference.action.Disable();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        rb.MovePosition(
            rb.position + moveInput.normalized *
            currentSpeed * Time.fixedDeltaTime
        );

        Rotate();
    }

    private void OnEnable()
    {
        moveActionReference.action.performed += OnMovePerformed;
        moveActionReference.action.canceled += OnMoveCancelled;

        sprintActionReference.action.started += Sprint;
        sprintActionReference.action.canceled += SprintStop;
    }

    private void OnDisable()
    {
        moveActionReference.action.performed -= OnMovePerformed;
        moveActionReference.action.canceled -= OnMoveCancelled;

        sprintActionReference.action.started -= Sprint;
        sprintActionReference.action.canceled -= SprintStop;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        moveInput = ctx.ReadValue<Vector2>();
        rb.gravityScale = 0;
    }

    private void OnMoveCancelled(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        moveInput = Vector2.zero;
        rb.gravityScale = gravity;
    }

    private void Sprint(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if (!isDragging)
            currentSpeed = swimFastSpeed;
    }

    private void SprintStop(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if (!isDragging)
            currentSpeed = swimSpeed;
    }

    private void Rotate()
    {
        if (moveInput != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(moveInput.y,moveInput.x) * Mathf.Rad2Deg;
             
            float newAngle = Mathf.LerpAngle(rb.rotation,targetAngle,rotationSpeed * Time.fixedDeltaTime);    
        rb.MoveRotation(newAngle);
        }
    }

    public void DraggedObject(bool check)
    {
        if (!IsOwner) return;

        isDragging = check;
        currentSpeed = isDragging ? swimPenalty : swimSpeed;
    }
}

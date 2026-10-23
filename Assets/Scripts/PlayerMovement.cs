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

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        currentSpeed = swimSpeed;

        if (!IsOwner)
        {
            camera.enabled = false;
        }
    }


    private void FixedUpdate()
    {
        if (IsOwner)
        {
            return;
        }

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
        if (isDragging == false)
        {
            currentSpeed = swimFastSpeed;
        }
    }

    private void SprintStop(InputAction.CallbackContext ctx)
    {
        if (isDragging == false)
        {
            currentSpeed = swimSpeed;
        }
    }

    private void Rotate()
    {
        if (moveInput != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, moveInput);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            rb.MoveRotation(rotation);
        }
    }

    public void DraggedObject(bool check)
    {
        isDragging = check;

        if (isDragging == true)
        {
            currentSpeed = swimPenalty;
        }
        else
        {
            currentSpeed = swimSpeed;
        }
    }
}

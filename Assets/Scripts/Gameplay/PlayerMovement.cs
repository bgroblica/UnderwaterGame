
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
    [SerializeField] private Camera cameraPlayer;

    private NetworkVariable<Vector3> initialSpawnPosition = new NetworkVariable<Vector3>(new Vector3(float.MinValue, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> hasAppliedSpawn = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> canMove = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentSpeed;
    private bool isDragging = false;
    private bool matchStarted = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        currentSpeed = swimSpeed;
    }

    public override void OnNetworkSpawn()
    {
        initialSpawnPosition.OnValueChanged += OnSpawnPositionChanged;
        canMove.OnValueChanged += OnCanMoveChanged;

        if (cameraPlayer != null)
            cameraPlayer.gameObject.SetActive(IsOwner);

        if (IsOwner)
        {
            moveActionReference.action.Enable();
            sprintActionReference.action.Enable();

            if (initialSpawnPosition.Value.x != float.MinValue)
            {
                ApplySpawnPosition(initialSpawnPosition.Value);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        initialSpawnPosition.OnValueChanged -= OnSpawnPositionChanged;
        canMove.OnValueChanged -= OnCanMoveChanged;

        if (IsOwner)
        {
            moveActionReference.action.Disable();
            sprintActionReference.action.Disable();
        }
    }

    public void EnableMovement()
    {
        if (!IsServer)
            return;

        canMove.Value = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (!canMove.Value)
        {
            rb.gravityScale = 0f;
            return;
        }

        rb.gravityScale = gravity;

        rb.MovePosition(
            rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime
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

    public void AssignSpawnPosition(Vector3 position)
    {
        if (!IsServer) return;

        initialSpawnPosition.Value = position;

        if (IsOwner)
            ApplySpawnPosition(position);

        Debug.Log($"AssignSpawnPosition: Owner={OwnerClientId}, " +$"IsServer={IsServer}, Position={position}");
    }

    private void OnSpawnPositionChanged(Vector3 previous, Vector3 current)
    {

        if (!IsOwner) return;

        ApplySpawnPosition(current);
    }

    private void ApplySpawnPosition(Vector3 position)
    {
        if (!IsOwner)
            return;

        rb.position = position;
        transform.position = position;

        moveInput = Vector2.zero;

        if (IsServer)
        {
            hasAppliedSpawn.Value = true;
        }
        else
        {
            ConfirmSpawnServerRpc();
        }
        Debug.Log($"ApplySpawnPosition: Owner={OwnerClientId}, " + $"IsOwner={IsOwner}, Position={initialSpawnPosition.Value}");
    }

    [ServerRpc]
    private void ConfirmSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        Debug.Log(
            $"Spawn confirmation received from sender {senderId}. " +
            $"Player OwnerClientId: {OwnerClientId}"
        );

        if (senderId != OwnerClientId)
        {
            Debug.LogError("Spawn confirmation sender doesn't match owner!");
            return;
        }

        hasAppliedSpawn.Value = true;

        Debug.Log(
            $"SERVER: hasAppliedSpawn for client {OwnerClientId} = " +
            $"{hasAppliedSpawn.Value}"
        );
        Debug.Log($"Spawn confirmed: Owner={OwnerClientId}, " + $"Sender={rpcParams.Receive.SenderClientId}");
    }

    public bool HasAppliedSpawn()
    {
        Debug.Log(
            $"Checking spawn confirmation for client {OwnerClientId}: " +
            $"{hasAppliedSpawn.Value}"
        );

        return hasAppliedSpawn.Value;
    }

    private void OnCanMoveChanged(bool previous, bool current)
    {
        Debug.Log($"Client {OwnerClientId}: canMove changed to {current}. " +$"IsOwner: {IsOwner}, IsServer: {IsServer}");
    }


    // MOVEMENT /////////////

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
        if (!IsOwner || !canMove.Value) return;

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

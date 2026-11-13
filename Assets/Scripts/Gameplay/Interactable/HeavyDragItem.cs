using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkRigidbody2D))]
public class HeavyDragItem : NetworkBehaviour, IInteractable
{
    private const ulong NoHolder = ulong.MaxValue;

    [Header("Movement")]
    [SerializeField] private float gravity = 0.3f;
    [SerializeField] private float gravityDrag = 0.1f;
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float offsetY = -2.5f;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;

    [Header("Materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material interactMaterial;

    private MeshRenderer mr;
    private Rigidbody2D rb;

    private NetworkVariable<ulong> holderClientId =
        new NetworkVariable<ulong>(
            NoHolder,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool CanInteract
    {
        get
        {
            if (!IsSpawned)
                return false;

            ulong localId = NetworkManager.Singleton.LocalClientId;

            return holderClientId.Value == NoHolder ||
                   holderClientId.Value == localId;
        }
    }

    public bool CanHighlight
    {
        get
        {
            return IsSpawned &&
                   holderClientId.Value == NoHolder;
        }
    }

    public override void OnNetworkSpawn()
    {
        mr = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody2D>();

        mr.material = defaultMaterial;

        holderClientId.OnValueChanged += OnHolderChanged;

        if (IsServer)
            rb.gravityScale = gravity;
    }

    private void OnHolderChanged(ulong previous, ulong current)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        PlayerMovement movement = NetworkManager.Singleton
            .LocalClient.PlayerObject
            .GetComponent<PlayerMovement>();

        if (movement == null)
            return;

        if (previous == localId)
            movement.DraggedObject(false);

        if (current == localId)
            movement.DraggedObject(true);
    }

    public override void OnNetworkDespawn()
    {
        holderClientId.OnValueChanged -= OnHolderChanged;

        if (mr != null)
            mr.material = defaultMaterial;
    }

    public void Interact()
    {
        if (!IsOwnerOfInteraction())
            return;

        if (holderClientId.Value == NoHolder)
        {
            RequestToggleRpc(true);
        }
        else if (holderClientId.Value == NetworkManager.Singleton.LocalClientId)
        {
            RequestToggleRpc(false);
        }
    }

    private bool IsOwnerOfInteraction()
    {
        return IsSpawned &&
               NetworkManager.Singleton != null;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestToggleRpc(
        bool wantsToGrab,
        RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
                senderId, out NetworkClient client))
            return;

        if (client.PlayerObject == null)
            return;

        if (wantsToGrab)
        {
            if (holderClientId.Value != NoHolder)
                return;

            float distance = Vector2.Distance(
                transform.position,
                client.PlayerObject.transform.position
            );

            if (distance > interactionDistance)
                return;

            holderClientId.Value = senderId;
            rb.gravityScale = gravityDrag;
        }
        else
        {
            if (holderClientId.Value != senderId)
                return;

            holderClientId.Value = NoHolder;
            rb.gravityScale = gravity;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer || holderClientId.Value == NoHolder)
            return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
                holderClientId.Value, out NetworkClient client))
            return;

        if (client.PlayerObject == null)
            return;

        Vector2 targetPosition =
            (Vector2)client.PlayerObject.transform.position +
            Vector2.up * offsetY;

        Vector2 newPosition = Vector2.Lerp(
            rb.position,
            targetPosition,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    public void OnEnter()
    {
        if (CanHighlight)
            mr.material = interactMaterial;
    }

    public void OnExit()
    {
        mr.material = defaultMaterial;
    }
}

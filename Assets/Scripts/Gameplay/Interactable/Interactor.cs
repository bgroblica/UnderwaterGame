
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Interactor : NetworkBehaviour
{
    private InputAction interactInput;

    private IInteractable currentInteractable;
    private bool isHighlighted;

    private void Start()
    {
        if (!IsOwner)
            return;

        interactInput = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        if (!IsOwner || interactInput == null)
            return;

        // Update highlighting when network availability changes.
        if (currentInteractable != null)
        {
            bool shouldHighlight = currentInteractable.CanHighlight;

            if (shouldHighlight != isHighlighted)
            {
                isHighlighted = shouldHighlight;

                if (isHighlighted)
                    currentInteractable.OnEnter();
                else
                    currentInteractable.OnExit();
            }
        }

        // Handle interaction.
        if (interactInput.WasPressedThisFrame() &&
            currentInteractable != null &&
            currentInteractable.CanInteract)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner)
            return;

        IInteractable interactable =
            collision.GetComponent<IInteractable>();

        if (interactable == null || currentInteractable != null)
            return;

        currentInteractable = interactable;

        if (currentInteractable.CanHighlight)
        {
            currentInteractable.OnEnter();
            isHighlighted = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsOwner)
            return;

        IInteractable interactable =
            collision.GetComponent<IInteractable>();

        if (interactable == null || interactable != currentInteractable)
            return;

        if (isHighlighted)
            currentInteractable.OnExit();

        currentInteractable = null;
        isHighlighted = false;
    }

    private void OnDisable()
    {
        if (isHighlighted && currentInteractable != null)
            currentInteractable.OnExit();

        currentInteractable = null;
        isHighlighted = false;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    private InputAction interactInput;

    private IInteractable currentInteractable;

    private bool hasOverride = false;

    private void Start()
    {
        interactInput = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        if (interactInput.WasPressedThisFrame() && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if (interactable != null && hasOverride == false)
        {
            currentInteractable = interactable;
            currentInteractable.OnEnter();
            Debug.Log("Current Interactable is" + currentInteractable);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if (interactable != null && interactable == currentInteractable && hasOverride == false)
        {
            currentInteractable.OnExit();
            currentInteractable = null;
            Debug.Log("Current Interactable is" + currentInteractable);
        }
    }

    public void CurrentInteractorOverride(IInteractable i)
    {
        currentInteractable = i;

        hasOverride = true;
    }

    public void OverrideStop()
    {
        currentInteractable = null;

        hasOverride = false;
    }
}

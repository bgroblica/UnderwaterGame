public interface IInteractable
{
    bool CanInteract { get; }
    bool CanHighlight { get; }

    void Interact();
    void OnEnter();
    void OnExit();
}
using UnityEngine;

public class PickupTest : MonoBehaviour, IInteractable
{
    [SerializeField] private Material deafultMaterial;
    [SerializeField] private Material interactMaterial;
    private MeshRenderer mr;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mr.material = deafultMaterial;
    }
    public void Interact()
    {
        PickUp();
    }

    public void OnEnter()
    {
        mr.material = interactMaterial;
    }

    public void OnExit()
    {
        mr.material = deafultMaterial;
    }

    private void PickUp()
    {
        Destroy(gameObject);
    }
}

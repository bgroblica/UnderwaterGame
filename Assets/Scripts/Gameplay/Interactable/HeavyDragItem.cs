using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HeavyDragItem : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Interactor interactor;
    [SerializeField] private float gravity = 0.3f;
    [SerializeField] private float gravityDrag = 0.1f;
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float offsetY = -2.5f;

    [SerializeField] private Material deafultMaterial;
    [SerializeField] private Material interactMaterial;
    private MeshRenderer mr;
   
    private Rigidbody2D rb;

    private CircleCollider2D cc;
   
    private bool isDragged = false;
   
    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mr.material = deafultMaterial;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        cc = GetComponent<CircleCollider2D>();
    }
   
    public void Interact()
    {
        isDragged = !isDragged;
   
        if (isDragged == true)
        {
            playerMovement.DraggedObject(true);
            rb.gravityScale = gravityDrag;
            mr.material = deafultMaterial;
            cc.enabled = false;
            interactor.CurrentInteractorOverride(this);
        }
        else
        {
            playerMovement.DraggedObject(false);
            rb.gravityScale = gravity;
            cc.enabled = true;
        }
    }
   
    public void OnEnter()
    {
        if (isDragged == true) 
            return;
        mr.material = interactMaterial;
    }
   
    public void OnExit()
    {
        mr.material = deafultMaterial;
    }
   
    private void Update()
    {
        if (isDragged == true)
        {
            float x = Mathf.Lerp(transform.position.x, playerMovement.transform.position.x, speed * Time.deltaTime);
            float y = Mathf.Lerp(transform.position.y, playerMovement.transform.position.y + offsetY, speed * Time.deltaTime);

            transform.position = new Vector3(x, y, 0);
        }
    }
}

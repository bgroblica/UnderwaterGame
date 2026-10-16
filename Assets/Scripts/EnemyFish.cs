using UnityEngine;

public class EnemyFish : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;

    private int targetPoint;
    [SerializeField] private float speed;
    [SerializeField] private float speedRotate;

    private Rigidbody2D rb;
    public Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPoint = 0;
    }

    private void Update()
    {
      //  if (transform.position == patrolPoints[targetPoint].position)
      //  {
      //      changeWaypoint();
      //  }
      //  transform.position = Vector3.MoveTowards(transform.position, patrolPoints[targetPoint].position, speed * Time.deltaTime);

      Vector2 direction = (player.position - transform.position).normalized;
      rb.linearVelocity = direction * speed * Time.deltaTime;
    }

    private void changeWaypoint()
    {
        targetPoint++;
        if (targetPoint >= patrolPoints.Length)
        {
            targetPoint = 0;
        }
    }
}

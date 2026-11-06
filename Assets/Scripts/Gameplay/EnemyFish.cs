using UnityEngine;

public class EnemyFish : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform[] patrolPoints;

    private int targetPoint;

    [Header("Fish Stats")]
    [SerializeField] private float rangeTarget = 0.8f;
    [SerializeField] private float speed = 4;
    [SerializeField] private float speedChase = 10;

    private float swimSpeed;

    [Header("Attack Stats")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float oxygenDamage = 10f;
    [SerializeField] private float timeToChasesAgain = 3f;

    private Rigidbody2D rb;
    private CircleCollider2D cc;
    private Vector2 movement;
    private Timer timer;

    public Transform player;
    public bool isChasing = false;
    public bool canChase = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CircleCollider2D>();

        cc.radius = detectRange;
        targetPoint = 0;
        timer = FindFirstObjectByType<Timer>();

        swimSpeed = speed;
    }

    private void Update()
    {
        Vector3 target = patrolPoints[targetPoint].position;

        if (isChasing == true)
        {
            swimSpeed = speedChase;
            target = player.position;
        }
        else
        {
            swimSpeed = speed;
            target = patrolPoints[targetPoint].position;
        }

        float dist = Vector2.Distance(target, transform.position);

        if (dist <= rangeTarget)
        {
          ChangeWaypoint();
          if (isChasing == true)
          {
            TryAttacking();
          }
        }

        Vector2 direction = target - transform.position;
        direction.Normalize();
        movement = direction;

        MoveEnemy(movement);
    }

    void MoveEnemy(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position + (direction * swimSpeed * Time.fixedDeltaTime));
    }

    private void ChangeWaypoint()
    {
        targetPoint++;
        if (targetPoint >= patrolPoints.Length)
        {
            targetPoint = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player") & canChase == true)
        {
            isChasing = true;
        }
    }

    private void TryAttacking()
    {
        timer.timer -= oxygenDamage;
        isChasing = false;
        canChase = false;
        Invoke("EnableChasing", timeToChasesAgain);
    }
    private void EnableChasing()
    {
        canChase = true; 
    }
}

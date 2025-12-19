using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ObstacleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private string wallTag = "Walls";

    private Rigidbody rb;
    private Vector3 currentDirection;
    private float timeToNextDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // We want gravity so they can fall onto the plane
        rb.useGravity = true;
        rb.isKinematic = false;

        // Keep them upright but allow full movement
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        // Helps avoid tunneling through walls
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        PickNewDirection();
    }

    private void FixedUpdate()
    {
        timeToNextDirection -= Time.fixedDeltaTime;
        if (timeToNextDirection <= 0f)
        {
            PickNewDirection();
        }

        // Horizontal movement controlled by us, vertical (Y) from gravity
        Vector3 horizontalVel = currentDirection * moveSpeed;
        rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
    }

    private void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;

        currentDirection = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;
        timeToNextDirection = directionChangeInterval;

        if (currentDirection != Vector3.zero)
        {
            transform.forward = currentDirection;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[EnemyMovement] {name} collided with {collision.gameObject.name}, tag={collision.gameObject.tag}");
    }

}

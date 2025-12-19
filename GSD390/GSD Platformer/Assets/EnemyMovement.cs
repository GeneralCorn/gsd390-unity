using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform knifeRoot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private string wallTag = "Walls";

    [Header("Sword Swing")]
    [SerializeField] private float swingAngle = 30f;
    [SerializeField] private float swingSpeed = 5f;

    private Transform player;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (knifeRoot == null)
        {
            knifeRoot = transform; // default: rotate the whole enemy
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[EnemyMovement] No GameObject tagged 'Player' found.");
        }
    }

    private void FixedUpdate()
    {
        // Don't move if game over
        if (GameManager.Instance != null && GameManager.Instance.GameEnded)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (player == null) return;

        // Direction to player in XZ plane
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 dir = toPlayer.normalized;

        // Move toward player (horizontal only)
        Vector3 horizontalVel = dir * moveSpeed;
        rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        Quaternion baseRot = Quaternion.LookRotation(dir, Vector3.up);

        //Swinging
        float swingOffset = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        Quaternion swingRot = Quaternion.AngleAxis(swingOffset, Vector3.up);

        Quaternion finalRot = baseRot * swingRot;
        knifeRoot.rotation = finalRot;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(wallTag) && collision.contactCount > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            normal.y = 0f;

            if (normal.sqrMagnitude > 0.0001f)
            {
                normal.Normalize();
                // small push away from wall
                rb.position += normal * 0.05f;
            }
        }
    }
}

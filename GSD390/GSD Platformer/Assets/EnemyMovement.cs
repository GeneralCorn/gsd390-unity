using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform knifeRoot;   // part that visually swings / points knife

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private string wallTag = "Walls";

    [Header("Rotation")]
    [SerializeField] private float turnSpeedDegrees = 75f; // how fast it can rotate per second

    [Header("Sword Swing")]
    [SerializeField] private float swingAngle = 30f;  // max degrees left/right
    [SerializeField] private float swingSpeed = 5f;   // how fast it swings

    private Transform player;
    private Rigidbody rb;
    private Quaternion facingRot;

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
            knifeRoot = transform;
        }

        facingRot = knifeRoot.rotation;
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
        // Stop if game ended
        if (GameManager.Instance != null && GameManager.Instance.GameEnded)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 desiredDir = toPlayer.normalized;
        Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
        float maxStep = turnSpeedDegrees * Time.fixedDeltaTime;
        facingRot = Quaternion.RotateTowards(facingRot, targetRot, maxStep);


        Vector3 moveDir = facingRot * Vector3.forward;
        moveDir.y = 0f;
        moveDir.Normalize();

        Vector3 horizontalVel = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);

        float swingOffset = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        Quaternion swingRot = Quaternion.AngleAxis(swingOffset, Vector3.up);

        Quaternion finalRot = facingRot * swingRot;
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
                rb.position += normal * 0.05f;
            }
        }
    }
}

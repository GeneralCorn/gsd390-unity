using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static int PlayerCount = 0;

    [SerializeField] private float rotSpeed = 90f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Color[] possibleColors;

    [Header("Tags")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Fall Settings")]
    [SerializeField] private float fallDeathDelay = 1f;

    private Coroutine fallCoroutine;
    private Renderer _renderer;

    private bool isDead = false;
    private int groundContactCount = 0;

    public float RotationSpeed
    {
        get => rotSpeed;
        private set => rotSpeed = value;
    }

    private void Awake()
    {
        PlayerCount++;
        Debug.Log($"[PlayerController] Awake: there are now {PlayerCount} Player(s) using this script.");
    }

    private void Start()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer != null && possibleColors != null && possibleColors.Length > 0)
        {
            Color currColor = GetRandomElement(possibleColors);
            _renderer.material.color = currColor;
            Debug.Log($"[PlayerController]: The color has now been switched to {currColor}");
        }

        Debug.Log($"[PlayerController]: Rotation Speed: {rotSpeed}, moveSpeed: {moveSpeed}");
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Stop movement if dead or game has ended
        if (isDead || (GameManager.Instance != null && GameManager.Instance.GameEnded))
            return;

        // Continuous rotation
        transform.Rotate(0f, RotationSpeed * Time.deltaTime, 0f);

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.upArrowKey.isPressed) vertical += 1f;

        Vector3 move = new Vector3(horizontal, 0f, vertical);

        if (move.sqrMagnitude > 0f)
        {
            move = move.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            RotationSpeed = -RotationSpeed;
            Debug.Log($"[PlayerController] Toggled rotation direction. New RotationSpeed = {RotationSpeed}");
        }
    }

    private T GetRandomElement<T>(T[] array)
    {
        int index = Random.Range(0, array.Length);
        return array[index];
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // touching the ground
        if (other.CompareTag(groundTag))
        {
            groundContactCount++;

            if (fallCoroutine != null)
            {
                StopCoroutine(fallCoroutine);
                fallCoroutine = null;
            }
        }

        // Touching an enemy → instant death
        if (other.CompareTag(enemyTag))
        {
            Die("Hit enemy: " + other.name);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Leaving the ground
        if (other.CompareTag(groundTag))
        {
            groundContactCount--;

            // If we’re no longer touching ANY ground, start delayed fall death
            if (groundContactCount <= 0 &&
                !isDead &&
                (GameManager.Instance == null || !GameManager.Instance.GameEnded) &&
                fallCoroutine == null)
            {
                fallCoroutine = StartCoroutine(FallDeathCountdown());
            }
        }
    }

    private System.Collections.IEnumerator FallDeathCountdown()
    {
        float t = 0f;

        while (t < fallDeathDelay)
        {
            if (isDead || (GameManager.Instance != null && GameManager.Instance.GameEnded))
            {
                fallCoroutine = null;
                yield break;
            }

            // If we touched ground again, cancel falling
            if (groundContactCount > 0)
            {
                fallCoroutine = null;
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        fallCoroutine = null;

        if (!isDead && groundContactCount <= 0)
        {
            Die("Fell off the ground (after delay)");
        }
    }

    private void Die(string reason)
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"[PlayerController] Player died: {reason}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
    }

    private void OnDestroy()
    {
        PlayerCount--;
    }
}

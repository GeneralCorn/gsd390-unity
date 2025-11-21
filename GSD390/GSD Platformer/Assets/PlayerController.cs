using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;  // for TMP_Text (TextMeshPro)

public class PlayerController : MonoBehaviour
{
    public static int PlayerCount = 0;

    [SerializeField] private float rotSpeed = 90f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Color[] possibleColors;

    // Tags in your scene
    [Header("Tags")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private string enemyTag = "Enemy";

    // Timer / UI
    [Header("Timer & UI")]
    [SerializeField] private float roundTime = 10f;  // countdown from 10
    [SerializeField] private TMP_Text eventsText;    // tagged "Events"
    [SerializeField] private TMP_Text timerText;     // tagged "Timer"
    [SerializeField]
    private float fallDeathDelay = 1f;
    private Coroutine fallCoroutine;
    private Renderer _renderer;

    // State
    private float timeRemaining;
    private bool isDead = false;
    private bool hasWon = false;
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

        // Auto-find UI by tag if not wired in Inspector
        if (eventsText == null)
        {
            GameObject eventsObj = GameObject.FindWithTag("Events");
            if (eventsObj != null)
                eventsText = eventsObj.GetComponent<TMP_Text>();
        }

        if (timerText == null)
        {
            GameObject timerObj = GameObject.FindWithTag("Timer");
            if (timerObj != null)
                timerText = timerObj.GetComponent<TMP_Text>();
        }
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

        // init timer
        timeRemaining = roundTime;
        if (eventsText != null) eventsText.text = ""; // start with empty events text
        UpdateTimerUI();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // If game over (win or death) -> only allow restart
        if (isDead || hasWon)
        {
            if (keyboard.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        // Countdown
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            if (!hasWon && !isDead)
            {
                Win();
            }
        }
        UpdateTimerUI();

        // Normal movement & rotation (only while alive & before win)
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

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // integer countdown (10, 9, 8, ...)
            int displayTime = Mathf.CeilToInt(timeRemaining);
            timerText.text = displayTime.ToString();
        }
    }

    private T GetRandomElement<T>(T[] array)
    {
        int index = Random.Range(0, array.Length);
        return array[index];
    }

    // -------- Collision detection --------

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Touching the ground
        if (other.CompareTag(groundTag))
        {
            groundContactCount++;

            // Cancel pending fall-death if we land again
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
            if (groundContactCount <= 0 && !isDead && !hasWon && fallCoroutine == null)
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
            // If game ended for any reason, stop
            if (isDead || hasWon)
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

        // Still not on ground after delay → death
        if (!isDead && !hasWon && groundContactCount <= 0)
        {
            Die("Fell off the ground (after delay)");
        }
    }



    // -------- Game state helpers --------

    private void Die(string reason)
    {
        if (isDead || hasWon) return;

        isDead = true;
        Debug.Log($"[PlayerController] Player died: {reason}");

        if (eventsText != null)
        {
            eventsText.text = "You Died\nPress R to Restart";
        }
    }

    private void Win()
    {
        if (hasWon || isDead) return;

        hasWon = true;
        Debug.Log("[PlayerController] Player survived — You Win!");

        if (eventsText != null)
        {
            eventsText.text = "You Win!\nPress R to Restart";
        }
    }

    private void OnDestroy()
    {
        PlayerCount--;
    }
}

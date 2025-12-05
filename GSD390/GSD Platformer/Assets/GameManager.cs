using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gamePanel;

    [Header("Timer Settings")]
    [SerializeField] private float roundTime = 10f;

    public bool GameEnded { get; private set; } = false;
    public bool GameStarted { get; private set; } = false;

    private bool isPaused = false;
    private float timeRemaining;

    // Found automatically under gamePanel
    private TMP_Text eventsText;   // tagged "Events"
    private TMP_Text timerText;    // tagged "Timer"
    private Button pauseButton;  // tagged "Pause"

    // Found automatically under pausePanel
    private Button continueButton; // tagged "Continue"
    private Button restartButton;  // tagged "Restart"

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CacheUIReferences();
    }

    private void CacheUIReferences()
    {
        if (gamePanel == null)
        {
            Debug.LogError("[GameManager] gamePanel is not assigned in inspector.");
        }
        else
        {
            TMP_Text[] texts = gamePanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t.CompareTag("Events"))
                    eventsText = t;
                else if (t.CompareTag("Timer"))
                    timerText = t;
            }

            if (eventsText == null)
                Debug.LogError("[GameManager] Could not find Events TMP_Text under gamePanel. Tag it 'Events'.");

            if (timerText == null)
                Debug.LogError("[GameManager] Could not find Timer TMP_Text under gamePanel. Tag it 'Timer'.");

            Button[] hudButtons = gamePanel.GetComponentsInChildren<Button>(true);
            foreach (var b in hudButtons)
            {
                if (b.CompareTag("Pause"))
                {
                    pauseButton = b;
                    break;
                }
            }

            if (pauseButton == null)
            {
                Debug.LogError("[GameManager] Could not find Pause Button under gamePanel. Tag it 'Pause'.");
            }
            else
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(TogglePause);
            }
        }

        // --- PausePanel buttons: Continue + Restart ---
        if (pausePanel == null)
        {
            Debug.LogError("[GameManager] pausePanel is not assigned in inspector.");
        }
        else
        {
            Button[] pauseButtons = pausePanel.GetComponentsInChildren<Button>(true);
            foreach (var b in pauseButtons)
            {
                if (b.CompareTag("Continue"))
                    continueButton = b;
                else if (b.CompareTag("Restart"))
                    restartButton = b;
            }

            if (continueButton == null)
                Debug.LogError("[GameManager] Could not find Continue button under pausePanel. Tag it 'Continue'.");
            else
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ResumeGame);
            }

            if (restartButton == null)
                Debug.LogError("[GameManager] Could not find Restart button under pausePanel. Tag it 'Restart'.");
            else
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(Restart);
            }
        }
    }

    private void Start()
    {
        timeRemaining = roundTime;

        if (eventsText != null)
            eventsText.text = "";

        UpdateTimerUI();

        GameStarted = false;
        GameEnded = false;
        isPaused = false;

        Time.timeScale = 0f;

        if (startPanel != null) startPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);  // HUD hidden before start
        if (pausePanel != null) pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.interactable = false; // can’t pause before game starts
    }

    private void Update()
    {
        // START: press Space to begin
        if (!GameStarted && !GameEnded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
            return;
        }

        // AFTER END: only allow restart
        if (GameEnded)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
            return;
        }

        // Optional keyboard pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused) return;

        // Timer while playing
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            PlayerWon();
        }
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int displayTime = Mathf.CeilToInt(timeRemaining);
            timerText.text = displayTime.ToString();
        }
    }

    // -------- Game state / UI control --------

    public void StartGame()
    {
        GameStarted = true;
        GameEnded = false;
        isPaused = false;

        Time.timeScale = 1f;

        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.interactable = true;
    }

    public void TogglePause()
    {
        if (GameEnded || !GameStarted)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    private void ResumeGame()
    {
        if (GameEnded) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void PlayerDied()
    {
        if (GameEnded) return;
        GameEnded = true;
        isPaused = false;

        Time.timeScale = 0f;
        Debug.Log("[GameManager] Player died.");

        if (eventsText != null)
            eventsText.text = "You Died!\nPress R to Restart";
        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.interactable = false;
    }

    private void PlayerWon()
    {
        if (GameEnded) return;
        GameEnded = true;
        isPaused = false;

        Time.timeScale = 0f;
        Debug.Log("[GameManager] Player survived — You Win!");

        if (eventsText != null)
            eventsText.text = "You Win!\nPress R to Restart";

        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.interactable = false;
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}

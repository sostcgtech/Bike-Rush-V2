using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject startScreen;              // Panel with start UI
    public TextMeshProUGUI startText;           // "TAP TO START" text
    public TextMeshProUGUI titleText;           // Optional game title

    [Header("Game References")]
    public BikeMotorOnly bikeController;        // Reference to your bike script

    [Header("Start Screen Settings")]
    public bool animateStartText = true;        // Pulse/fade animation on start text
    public float animationSpeed = 2f;           // Speed of text animation

    private bool gameStarted = false;
    private float animTime = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Ensure game starts paused
        Time.timeScale = 0f;
        gameStarted = false;

        // Show start screen
        if (startScreen != null)
            startScreen.SetActive(true);

        // Disable bike controller until game starts
        if (bikeController != null)
            bikeController.enabled = false;
    }

    void Update()
    {
        if (!gameStarted)
        {
            // Animate start text (optional pulsing effect)
            if (animateStartText && startText != null)
            {
                animTime += Time.unscaledDeltaTime * animationSpeed;
                float alpha = Mathf.Lerp(0.5f, 1f, (Mathf.Sin(animTime) + 1f) / 2f);
                Color c = startText.color;
                c.a = alpha;
                startText.color = c;
            }

            // Check for tap/click to start
            bool inputDetected = false;

            // Mouse/Touch input
            if (Input.GetMouseButtonDown(0))
            {
                inputDetected = true;
            }

            // Mobile touch input
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                inputDetected = true;
            }

            // Keyboard input (spacebar or Enter)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                inputDetected = true;
            }

            if (inputDetected)
            {
                StartGame();
            }
        }
    }

    void StartGame()
    {
        gameStarted = true;

        // Hide start screen
        if (startScreen != null)
            startScreen.SetActive(false);

        // Enable bike controller
        if (bikeController != null)
            bikeController.enabled = true;

        // Resume game time
        Time.timeScale = 1f;

        // Start spawning traffic and obstacles
        if (WorldManager.Instance != null)
            WorldManager.Instance.OnGameStart();

        Debug.Log("Game Started!");
    }

    // Called when player crashes
    public void PlayerCrashed()
    {
        if (!gameStarted)
            return;

        Debug.Log("Player Crashed!");

        // Stop spawning
        if (WorldManager.Instance != null)
            WorldManager.Instance.StopAllSystems();

        // Pause the game
        Time.timeScale = 0f;

        // Optional: Show game over screen here
        // You can add a game over UI panel and show it

        // For now, restart after a delay (you can customize this)
        Invoke("RestartGame", 2f);
    }

    // Public method to restart game (call this from a restart button)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // Check if game has started (useful for other scripts)
    public bool IsGameStarted()
    {
        return gameStarted;
    }
}

//using UnityEngine;
//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;
//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }
//    public void PlayerCrashed() { Debug.Log("Crashed"); }
//}



//using UnityEngine;
//using UnityEngine.UI;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;
//    public Text distanceText;
//    public GameObject gameOverPanel;
//    public Transform bike;
//    float startZ;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        startZ = bike.position.z;
//        if (gameOverPanel) gameOverPanel.SetActive(false);
//    }

//    void Update()
//    {
//        float dist = bike.position.z - startZ;
//        if (distanceText != null) distanceText.text = Mathf.FloorToInt(dist).ToString() + " m";
//    }

//    public void PlayerCrashed()
//    {
//        // show game over UI
//        if (gameOverPanel) gameOverPanel.SetActive(true);
//        // you can pause time or show effects
//    }

//    public void Restart()
//    {
//        // reload scene or reset variables
//        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
//    }
//}

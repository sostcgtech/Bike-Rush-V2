using UnityEngine;
using UnityEngine.UI;

public class UIButtonManager : MonoBehaviour
{
    [Header("Button References")]
    public Button startButton;      // NEW: Start button on start screen
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;  // Optional

    void Start()
    {
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            Debug.Log("Start button clicked");
        }
    }

    void OnPauseClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
            Debug.Log("Pause button clicked");
        }
    }

    void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
            Debug.Log("Resume button clicked");
        }
    }

    void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
            Debug.Log("Restart button clicked");
        }
    }

    void OnQuitClicked()
    {
        Debug.Log("Quit button clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);

        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OnPauseClicked);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }
}
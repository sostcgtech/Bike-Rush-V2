using UnityEngine;

/// <summary>
/// World Manager - Central controller for traffic and obstacle systems.
/// Coordinates spawning, game state, and difficulty.
/// </summary>
public class WorldManager : MonoBehaviour
{
    #region Singleton
    public static WorldManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [Header("System References")]
    [Tooltip("Traffic pooling system (auto-detected if not set)")]
    public TrafficPooling trafficPooling;
    [Tooltip("Obstacle pooling system (auto-detected if not set)")]
    public ObstaclePooling obstaclePooling;

    [Header("Startup")]
    [Tooltip("Delay before spawning starts after game begins")]
    public float startDelay = 1.5f;

    void Start()
    {
        // Auto-find systems if not assigned
        if (trafficPooling == null)
            trafficPooling = FindObjectOfType<TrafficPooling>();
        if (obstaclePooling == null)
            obstaclePooling = FindObjectOfType<ObstaclePooling>();
    }

    /// <summary>
    /// Call when game starts - begins spawning after delay
    /// </summary>
    public void OnGameStart()
    {
        Invoke(nameof(StartAllSystems), startDelay);
    }

    void StartAllSystems()
    {
        if (trafficPooling != null)
            trafficPooling.StartTraffic();
        
        if (obstaclePooling != null)
            obstaclePooling.StartSpawning();

        Debug.Log("WorldManager: All spawning systems started");
    }

    /// <summary>
    /// Stop all spawning (game over, pause)
    /// </summary>
    public void StopAllSystems()
    {
        if (trafficPooling != null)
            trafficPooling.StopTraffic();
        
        if (obstaclePooling != null)
            obstaclePooling.StopSpawning();
    }

    /// <summary>
    /// Pause spawning but keep existing objects
    /// </summary>
    public void PauseAllSystems()
    {
        if (trafficPooling != null)
            trafficPooling.PauseTraffic();
    }

    /// <summary>
    /// Resume spawning
    /// </summary>
    public void ResumeAllSystems()
    {
        if (trafficPooling != null)
            trafficPooling.ResumeTraffic();
    }

    /// <summary>
    /// Reset for new game
    /// </summary>
    public void ResetAll()
    {
        StopAllSystems();
        
        if (obstaclePooling != null)
            obstaclePooling.ResetDifficulty();
    }
}

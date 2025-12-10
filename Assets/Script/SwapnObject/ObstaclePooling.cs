using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Obstacle Pooling System - Manages road obstacle spawning and recycling.
/// Handles roadblocks, barriers, cones, tire busters, etc.
/// </summary>
public class ObstaclePooling : MonoBehaviour
{
    #region Singleton
    public static ObstaclePooling Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [Header("Lane Configuration")]
    [Tooltip("Lane transforms for obstacle placement")]
    public Transform[] lanes;

    [Header("Obstacle Types")]
    [Tooltip("Obstacle prefabs with spawn weights")]
    public ObstacleEntry[] obstacles;

    [System.Serializable]
    public class ObstacleEntry
    {
        public string name;
        public GameObject prefab;
        [Range(1, 10)]
        public int poolSize = 2;
        [Range(0f, 1f)]
        public float spawnWeight = 1f;
    }

    [Header("Spawn Settings")]
    [Tooltip("Minimum distance between obstacles")]
    public float obstacleSpacing = 40f;
    [Tooltip("Distance ahead of player to spawn")]
    public float spawnDistance = 100f;
    [Tooltip("Distance behind player to recycle")]
    public float recycleDistance = 30f;

    [Header("Difficulty")]
    [Tooltip("Chance to spawn obstacle cluster (multiple in a row)")]
    [Range(0f, 0.5f)]
    public float clusterChance = 0.15f;
    [Tooltip("Reduce spacing over time")]
    public bool increaseDifficulty = true;
    [Tooltip("Minimum spacing (difficulty cap)")]
    public float minSpacing = 20f;

    [Header("References")]
    public Transform player;

    // Internal
    private List<RoadObstacle> obstaclePool = new List<RoadObstacle>();
    private float nextSpawnZ = 0f;
    private float totalWeight = 0f;
    private float gameStartTime;
    private bool isSpawning = false;

    void Start()
    {
        FindPlayer();
        CalculateWeights();
        InitializePool();
        gameStartTime = Time.time;
    }

    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) playerObj = GameObject.FindGameObjectWithTag("Motorcycle");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void CalculateWeights()
    {
        totalWeight = 0f;
        foreach (var obs in obstacles)
        {
            totalWeight += obs.spawnWeight;
        }
    }

    void InitializePool()
    {
        if (obstacles == null || obstacles.Length == 0) return;

        foreach (var entry in obstacles)
        {
            if (entry.prefab == null) continue;

            for (int i = 0; i < entry.poolSize; i++)
            {
                GameObject obsObj = Instantiate(entry.prefab, Vector3.zero, Quaternion.identity);
                
                // Reset scale to 1,1,1 (fix for prefabs with wrong scale)
                obsObj.transform.localScale = Vector3.one;
                
                obsObj.SetActive(false);
                obsObj.transform.SetParent(transform);

                RoadObstacle obstacle = obsObj.GetComponent<RoadObstacle>();
                if (obstacle == null)
                {
                    obstacle = obsObj.AddComponent<RoadObstacle>();
                }

                obstaclePool.Add(obstacle);
            }
        }

        Debug.Log($"ObstaclePooling: Initialized {obstaclePool.Count} obstacles");
    }

    void Update()
    {
        if (!isSpawning || player == null) return;

        ManageObstacles();
        CheckForNewSpawns();
    }

    void ManageObstacles()
    {
        foreach (var obstacle in obstaclePool)
        {
            if (obstacle == null || !obstacle.gameObject.activeSelf) continue;

            float distanceBehind = player.position.z - obstacle.transform.position.z;
            if (distanceBehind > recycleDistance)
            {
                obstacle.gameObject.SetActive(false);
            }
        }
    }

    void CheckForNewSpawns()
    {
        float currentSpacing = obstacleSpacing;
        
        // Increase difficulty over time
        if (increaseDifficulty)
        {
            float minutesPlayed = (Time.time - gameStartTime) / 60f;
            currentSpacing = Mathf.Max(minSpacing, obstacleSpacing - (minutesPlayed * 3f));
        }

        // Check if we should spawn new obstacle
        if (player.position.z + spawnDistance > nextSpawnZ)
        {
            SpawnObstacle();
            nextSpawnZ += currentSpacing;

            // Cluster spawn chance
            if (Random.value < clusterChance)
            {
                nextSpawnZ += 5f; // Small gap
                SpawnObstacle();
            }
        }
    }

    void SpawnObstacle()
    {
        if (lanes == null || lanes.Length == 0) return;

        // Find inactive obstacle
        RoadObstacle obstacle = GetInactiveObstacle();
        if (obstacle == null) return;

        // Random lane
        int laneIndex = Random.Range(0, lanes.Length);
        float laneX = lanes[laneIndex].position.x;
        float laneY = lanes[laneIndex].position.y;

        // Position
        Vector3 spawnPos = new Vector3(laneX, laneY, nextSpawnZ);
        obstacle.transform.position = spawnPos;
        obstacle.transform.rotation = Quaternion.identity;

        obstacle.gameObject.SetActive(true);
    }

    RoadObstacle GetInactiveObstacle()
    {
        // Weighted random selection
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        int targetIndex = 0;

        for (int i = 0; i < obstacles.Length; i++)
        {
            currentWeight += obstacles[i].spawnWeight;
            if (randomValue <= currentWeight)
            {
                targetIndex = i;
                break;
            }
        }

        // Find inactive obstacle of this type
        string targetName = obstacles[targetIndex].prefab.name;
        foreach (var obs in obstaclePool)
        {
            if (!obs.gameObject.activeSelf && obs.gameObject.name.StartsWith(targetName))
            {
                return obs;
            }
        }

        // Fallback: any inactive obstacle
        foreach (var obs in obstaclePool)
        {
            if (!obs.gameObject.activeSelf)
            {
                return obs;
            }
        }

        return null;
    }

    public void StartSpawning()
    {
        isSpawning = true;
        nextSpawnZ = player != null ? player.position.z + spawnDistance : 0f;
        gameStartTime = Time.time;
    }

    public void StopSpawning()
    {
        isSpawning = false;
        foreach (var obs in obstaclePool)
        {
            if (obs != null)
                obs.gameObject.SetActive(false);
        }
    }

    public void ResetDifficulty()
    {
        gameStartTime = Time.time;
    }
}

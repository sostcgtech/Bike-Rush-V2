using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Traffic Pooling System - Manages traffic vehicle spawning and recycling.
/// Based on Highway Racer approach with object pooling for optimal performance.
/// </summary>
public class TrafficPooling : MonoBehaviour
{
    #region Singleton
    public static TrafficPooling Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [Header("Lane Configuration")]
    [Tooltip("Lane transforms defining where vehicles can spawn (X position from each lane)")]
    public Transform[] lanes;

    [Header("Traffic Vehicles")]
    [Tooltip("Vehicle prefabs with pool count")]
    public VehiclePool[] vehiclePools;

    [System.Serializable]
    public class VehiclePool
    {
        public GameObject prefab;
        [Range(1, 10)]
        public int poolSize = 3;
    }

    [Header("Spawn Settings")]
    [Tooltip("Minimum distance ahead of player to spawn vehicles")]
    public float spawnDistanceMin = 80f;
    [Tooltip("Maximum distance ahead of player to spawn vehicles")]
    public float spawnDistanceMax = 200f;
    [Tooltip("Distance behind player before recycling vehicle")]
    public float recycleDistance = 25f;

    [Header("Vehicle Speed")]
    [Tooltip("Minimum vehicle speed")]
    public float speedMin = 8f;
    [Tooltip("Maximum vehicle speed")]
    public float speedMax = 18f;

    [Header("References")]
    [Tooltip("Player/Motorcycle transform (auto-detected if not set)")]
    public Transform player;

    // Internal
    private List<TrafficVehicle> vehiclePool = new List<TrafficVehicle>();
    private bool isSpawning = false;

    void Start()
    {
        FindPlayer();
        InitializePool();
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

    void InitializePool()
    {
        if (vehiclePools == null || vehiclePools.Length == 0)
        {
            Debug.LogWarning("TrafficPooling: No vehicle prefabs configured!");
            return;
        }

        foreach (var pool in vehiclePools)
        {
            if (pool.prefab == null) continue;

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject vehicleObj = Instantiate(pool.prefab, Vector3.zero, Quaternion.identity);
                
                // Reset scale to 1,1,1 (fix for prefabs with wrong scale)
                vehicleObj.transform.localScale = Vector3.one;
                
                vehicleObj.SetActive(false);
                vehicleObj.transform.SetParent(transform);

                TrafficVehicle vehicle = vehicleObj.GetComponent<TrafficVehicle>();
                if (vehicle == null)
                {
                    vehicle = vehicleObj.AddComponent<TrafficVehicle>();
                }

                vehiclePool.Add(vehicle);
            }
        }

        Debug.Log($"TrafficPooling: Initialized {vehiclePool.Count} vehicles");
    }

    void Update()
    {
        if (!isSpawning || player == null || lanes == null || lanes.Length == 0) return;

        ManageVehicles();
    }

    void ManageVehicles()
    {
        foreach (var vehicle in vehiclePool)
        {
            if (vehicle == null) continue;

            if (vehicle.gameObject.activeSelf)
            {
                // Vehicle is active - check if it should be recycled
                float distanceBehind = player.position.z - vehicle.transform.position.z;
                if (distanceBehind > recycleDistance)
                {
                    RepositionVehicle(vehicle);
                }
            }
            else
            {
                // Vehicle is inactive - spawn it
                RepositionVehicle(vehicle);
            }
        }
    }

    void RepositionVehicle(TrafficVehicle vehicle)
    {
        // Select random lane
        int laneIndex = Random.Range(0, lanes.Length);
        float laneX = lanes[laneIndex].position.x;
        float laneY = lanes[laneIndex].position.y;

        // Calculate spawn position ahead of player
        float spawnZ = player.position.z + Random.Range(spawnDistanceMin, spawnDistanceMax);
        Vector3 spawnPosition = new Vector3(laneX, laneY, spawnZ);

        // Reset scale to 1,1,1 (fix for prefabs with wrong scale)
        vehicle.transform.localScale = Vector3.one;

        // Set position and rotation (facing -Z direction, towards the player)
        vehicle.transform.position = spawnPosition;
        vehicle.transform.rotation = Quaternion.Euler(0, 180, 0);

        // Initialize with random speed
        float speed = Random.Range(speedMin, speedMax);
        vehicle.Setup(speed, laneIndex);

        // Activate vehicle
        vehicle.gameObject.SetActive(true);

        // Check for overlap with other vehicles
        if (CheckOverlap(vehicle))
        {
            vehicle.gameObject.SetActive(false);
        }
    }

    bool CheckOverlap(TrafficVehicle newVehicle)
    {
        foreach (var other in vehiclePool)
        {
            if (other == newVehicle || !other.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(newVehicle.transform.position, other.transform.position);
            if (distance < 12f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Start traffic spawning
    /// </summary>
    public void StartTraffic()
    {
        isSpawning = true;
    }

    /// <summary>
    /// Stop traffic and deactivate all vehicles
    /// </summary>
    public void StopTraffic()
    {
        isSpawning = false;
        foreach (var vehicle in vehiclePool)
        {
            if (vehicle != null)
                vehicle.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Pause traffic (vehicles stay but stop moving)
    /// </summary>
    public void PauseTraffic()
    {
        isSpawning = false;
    }

    /// <summary>
    /// Resume traffic
    /// </summary>
    public void ResumeTraffic()
    {
        isSpawning = true;
    }
}

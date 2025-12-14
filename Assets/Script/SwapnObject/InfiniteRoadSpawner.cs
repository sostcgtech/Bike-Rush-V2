//using System.Collections.Generic;
//using UnityEngine;

//public class InfiniteRoadSpawner : MonoBehaviour
//{
//    [Header("Road Settings")]
//    [SerializeField] private GameObject roadPrefab;
//    [SerializeField] private int initialRoadCount = 5;
//    [SerializeField] private float roadLength = 10f;

//    [Header("Obstacle Settings")]
//    [SerializeField] private GameObject[] obstaclePrefabs;   // Assign multiple obstacles
//    [SerializeField] private int obstaclesPerRoad = 2;       // How many obstacles per road
//    [SerializeField] private float laneWidth = 2f;           // Should match BikeMotorOnly

//    private readonly List<GameObject> activeRoads = new List<GameObject>();
//    private Vector3 nextSpawnPosition;
//    private int roadsPassed = 0;

//    private void Start()
//    {
//        nextSpawnPosition = transform.position;

//        for (int i = 0; i < initialRoadCount; i++)
//        {
//            SpawnRoad();
//        }
//    }

//    private void SpawnRoad()
//    {
//        GameObject newRoad = Instantiate(roadPrefab, nextSpawnPosition, transform.rotation);
//        activeRoads.Add(newRoad);

//        // Make sure road has trigger
//        BoxCollider trigger = newRoad.GetComponent<BoxCollider>();
//        if (trigger == null) trigger = newRoad.AddComponent<BoxCollider>();
//        trigger.isTrigger = true;

//        RoadT roadTrigger = newRoad.GetComponent<RoadT>();
//        if (roadTrigger == null) roadTrigger = newRoad.AddComponent<RoadT>();
//        roadTrigger.Initialize(this);

//        // Spawn obstacles on this road
//        SpawnObstacles(newRoad);

//        nextSpawnPosition += transform.forward * roadLength;
//    }

//    private void SpawnObstacles(GameObject road)
//    {
//        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
//        {
//            Debug.LogWarning("No obstacle prefabs assigned!");
//            return;
//        }

//        for (int i = 0; i < obstaclesPerRoad; i++)
//        {
//            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

//            int lane = Random.Range(0, 3);
//            float xPos = (lane - 1) * laneWidth;
//            float zPos = Random.Range(0.5f, roadLength - 0.5f);

//            Vector3 spawnPos = road.transform.position +
//                              road.transform.forward * zPos +
//                              road.transform.right * xPos;

//            GameObject obs = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, road.transform);
//            obs.tag = "Obstacle";

//            Debug.Log($"Spawned obstacle at {spawnPos}"); // Debug to verify position
//        }
//    }

//    public void OnRoadEntered()
//    {
//        roadsPassed++;
//        SpawnRoad();

//        if (roadsPassed >= 2 && activeRoads.Count > 0)
//        {
//            GameObject oldRoad = activeRoads[0];
//            activeRoads.RemoveAt(0);
//            Destroy(oldRoad);
//        }
//    }
//}


//work code
using System.Collections.Generic;
using UnityEngine;

public class InfiniteRoadSpawner : MonoBehaviour
{
    [Header("Road Settings")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private int initialRoadCount = 5;
    [SerializeField] private float roadLength = 10f;

    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;   // Assign multiple obstacles
    [SerializeField] private int obstaclesPerRoad = 2;       // How many obstacles per road
    [SerializeField] private float laneWidth = 2f;           // Should match BikeMotorOnly

    private readonly List<GameObject> activeRoads = new List<GameObject>();
    private Vector3 nextSpawnPosition;
    private int roadsPassed = 0;

    private void Start()
    {
        nextSpawnPosition = transform.position;

        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    private void SpawnRoad()
    {
        GameObject newRoad = Instantiate(roadPrefab, nextSpawnPosition, transform.rotation);
        activeRoads.Add(newRoad);

        // Make sure road has trigger
        BoxCollider trigger = newRoad.GetComponent<BoxCollider>();
        if (trigger == null) trigger = newRoad.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        RoadT roadTrigger = newRoad.GetComponent<RoadT>();
        if (roadTrigger == null) roadTrigger = newRoad.AddComponent<RoadT>();
        roadTrigger.Initialize(this);

        // Spawn obstacles on this road
        SpawnObstacles(newRoad);

        nextSpawnPosition += transform.forward * roadLength;
    }

    private void SpawnObstacles(GameObject road)
    {
        for (int i = 0; i < obstaclesPerRoad; i++)
        {
            // Pick a random obstacle prefab
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

            // Pick a random lane
            int lane = Random.Range(0, 3); // Assuming 3 lanes
            float xPos = (lane - 1) * laneWidth; // Left=-1, Center=0, Right=1

            // Pick a random Z position within the road length
            float zPos = Random.Range(0.5f, roadLength - 0.5f);

            // Instantiate obstacle
            Vector3 spawnPos = road.transform.position + road.transform.forward * zPos + new Vector3(xPos, 0, 0);
            GameObject obs = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, road.transform);
            obs.tag = "Obstacle";
        }
    }

    public void OnRoadEntered()
    {
        roadsPassed++;
        SpawnRoad();

        if (roadsPassed >= 2 && activeRoads.Count > 0)
        {
            GameObject oldRoad = activeRoads[0];
            activeRoads.RemoveAt(0);
            Destroy(oldRoad);
        }
    }
}


//using System.Collections.Generic;
//using UnityEngine;

//public class InfiniteRoadSpawner : MonoBehaviour
//{
//    [Header("Road Settings")]
//    [SerializeField] private GameObject roadPrefab;
//    [SerializeField] private int initialRoadCount = 5;
//    [SerializeField] private float roadLength = 10f;

//    private readonly List<GameObject> activeRoads = new List<GameObject>();
//    private Vector3 nextSpawnPosition;
//    private int roadsPassed = 0;

//    private void Start()
//    {
//        nextSpawnPosition = transform.position;

//        // Spawn initial roads
//        for (int i = 0; i < initialRoadCount; i++)
//        {
//            SpawnRoad();
//        }
//    }

//    private void SpawnRoad()
//    {
//        // Spawn road aligned to spawner's rotation
//        GameObject newRoad = Instantiate(
//            roadPrefab,
//            nextSpawnPosition,
//            transform.rotation   // <-- FIX: use spawner rotation
//        );

//        activeRoads.Add(newRoad);

//        // Make sure road has trigger
//        BoxCollider trigger = newRoad.GetComponent<BoxCollider>();
//        if (trigger == null)
//            trigger = newRoad.AddComponent<BoxCollider>();

//        trigger.isTrigger = true;

//        // Add road trigger script
//        RoadT roadTrigger = newRoad.GetComponent<RoadT>();
//        if (roadTrigger == null)
//            roadTrigger = newRoad.AddComponent<RoadT>();

//        roadTrigger.Initialize(this);

//        // Move spawn position forward based on spawner's forward direction
//        nextSpawnPosition += transform.forward * roadLength;   // <-- FIX: correct direction
//    }

//    public void OnRoadEntered()
//    {
//        roadsPassed++;

//        // Always spawn a new road ahead
//        SpawnRoad();

//        // Remove old road
//        if (roadsPassed >= 2 && activeRoads.Count > 0)
//        {
//            GameObject oldRoad = activeRoads[0];
//            activeRoads.RemoveAt(0);
//            Destroy(oldRoad);
//        }
//    }
//}

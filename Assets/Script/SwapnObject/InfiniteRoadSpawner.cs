//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class InfiniteRoadSpawner : MonoBehaviour
//{
//    [Header("Road Settings")]
//    [SerializeField] private GameObject roadPrefab;
//    [SerializeField] private int initialRoadCount = 5;
//    [SerializeField] private float roadLength = 10f;
//    [SerializeField] private Transform motorcycle;
//    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // Adjust this in inspector

//    private List<GameObject> activeRoads = new List<GameObject>();
//    private Vector3 nextSpawnPosition;
//    private Quaternion nextSpawnRotation;
//    private int roadsPassed = 0;
//    private Transform lastRoad;

//    void Start()
//    {
//        // If motorcycle not assigned, try to find it
//        if (motorcycle == null)
//        {
//            GameObject bike = GameObject.FindGameObjectWithTag("Player");
//            if (bike == null) bike = GameObject.FindGameObjectWithTag("Motorcycle");
//            if (bike != null) motorcycle = bike.transform;
//        }

//        // Start spawning from the spawner's position and rotation
//        nextSpawnPosition = transform.position;
//        nextSpawnRotation = transform.rotation;

//        for (int i = 0; i < initialRoadCount; i++)
//        {
//            SpawnRoad();
//        }
//    }

//    void SpawnRoad()
//    {
//        GameObject newRoad = Instantiate(roadPrefab, nextSpawnPosition, nextSpawnRotation);
//        activeRoads.Add(newRoad);

//        // Add trigger component if not already present
//        BoxCollider trigger = newRoad.GetComponent<BoxCollider>();
//        if (trigger == null)
//        {
//            trigger = newRoad.AddComponent<BoxCollider>();
//        }
//        trigger.isTrigger = true;

//        // Add the trigger handler component
//        RoadT roadTrigger = newRoad.GetComponent<RoadT>();
//        if (roadTrigger == null)
//        {
//            roadTrigger = newRoad.AddComponent<RoadT>();
//        }
//        roadTrigger.Initialize(this);

//        // Move spawn position forward based on current rotation
//        nextSpawnPosition += nextSpawnRotation * Vector3.forward * roadLength;

//        // Update rotation to follow motorcycle if available
//        if (motorcycle != null)
//        {
//            nextSpawnRotation = motorcycle.rotation;
//        }
//    }

//    public void OnRoadEntered()
//    {
//        roadsPassed++;

//        // Spawn new road ahead
//        SpawnRoad();

//        // Remove the first road (the one we just passed) after entering the second road
//        if (roadsPassed >= 2 && activeRoads.Count > 0)
//        {
//            GameObject oldRoad = activeRoads[0];
//            activeRoads.RemoveAt(0);
//            Destroy(oldRoad);
//        }
//    }
//}


using System.Collections.Generic;
using UnityEngine;

public class InfiniteRoadSpawner : MonoBehaviour
{
    [Header("Road Settings")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private int initialRoadCount = 5;
    [SerializeField] private float roadLength = 10f;

    private readonly List<GameObject> activeRoads = new List<GameObject>();
    private Vector3 nextSpawnPosition;
    private int roadsPassed = 0;

    private void Start()
    {
        nextSpawnPosition = transform.position;

        // Spawn initial roads
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    private void SpawnRoad()
    {
        // Spawn road aligned to spawner's rotation
        GameObject newRoad = Instantiate(
            roadPrefab,
            nextSpawnPosition,
            transform.rotation   // <-- FIX: use spawner rotation
        );

        activeRoads.Add(newRoad);

        // Make sure road has trigger
        BoxCollider trigger = newRoad.GetComponent<BoxCollider>();
        if (trigger == null)
            trigger = newRoad.AddComponent<BoxCollider>();

        trigger.isTrigger = true;

        // Add road trigger script
        RoadT roadTrigger = newRoad.GetComponent<RoadT>();
        if (roadTrigger == null)
            roadTrigger = newRoad.AddComponent<RoadT>();

        roadTrigger.Initialize(this);

        // Move spawn position forward based on spawner's forward direction
        nextSpawnPosition += transform.forward * roadLength;   // <-- FIX: correct direction
    }

    public void OnRoadEntered()
    {
        roadsPassed++;

        // Always spawn a new road ahead
        SpawnRoad();

        // Remove old road
        if (roadsPassed >= 2 && activeRoads.Count > 0)
        {
            GameObject oldRoad = activeRoads[0];
            activeRoads.RemoveAt(0);
            Destroy(oldRoad);
        }
    }
}

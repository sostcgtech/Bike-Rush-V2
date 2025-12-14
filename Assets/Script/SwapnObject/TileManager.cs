using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject[] tilePrefabs;
    public float tileLength = 1.983f;
    public int numberOfTiles = 5;

    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Spawn Settings")]
    public float spawnDistance = 35f;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public int obstaclesPerTile = 2;
    public float laneWidth = 2f;
    public float obstacleYOffset = 0f; // Adjust this in Inspector if cars float/sink

    private float zSpawn = 0f;
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
        }
    }

    private void Update()
    {
        if (playerTransform.position.z - spawnDistance > zSpawn - (numberOfTiles * tileLength))
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
            DeleteOldTiles();
        }
    }

    public void SpawnTile(int tileIndex)
    {
        GameObject newTile = Instantiate(
            tilePrefabs[tileIndex],
            new Vector3(0, 0, zSpawn),
            Quaternion.identity
        );
        activeTiles.Add(newTile);

        // Spawn obstacles on this tile
        SpawnObstaclesOnTile(newTile);

        zSpawn += tileLength;
    }

    private void SpawnObstaclesOnTile(GameObject tile)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        for (int i = 0; i < obstaclesPerTile; i++)
        {
            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

            // Pick random lane (0=left, 1=center, 2=right)
            int lane = Random.Range(0, 3);
            float xPos = (lane - 1) * laneWidth;

            // Random position along the tile
            float zPos = Random.Range(0.2f, tileLength - 0.2f);

            // Calculate spawn position with adjustable Y offset
            Vector3 spawnPos = tile.transform.position + new Vector3(xPos, obstacleYOffset, zPos);

            // Set rotation to X: 90, Y: 90
            Quaternion spawnRotation = Quaternion.Euler(90, 90, 0);

            GameObject obs = Instantiate(obstaclePrefab, spawnPos, spawnRotation, tile.transform);
            obs.tag = "Obstacle";
        }
    }

    private void DeleteOldTiles()
    {
        if (activeTiles.Count > 0 && activeTiles[0].transform.position.z < playerTransform.position.z - 20f)
        {
            Destroy(activeTiles[0]);
            activeTiles.RemoveAt(0);
        }
    }
}


//work with swapn but rotation are not good
//using UnityEngine;
//using System.Collections.Generic;

//public class TileManager : MonoBehaviour
//{
//    [Header("Tile Settings")]
//    public GameObject[] tilePrefabs;
//    public float tileLength = 1.983f;
//    public int numberOfTiles = 5;

//    [Header("Player Reference")]
//    public Transform playerTransform;

//    [Header("Spawn Settings")]
//    public float spawnDistance = 35f;

//    [Header("Obstacle Settings")]
//    public GameObject[] obstaclePrefabs;
//    public int obstaclesPerTile = 2;
//    public float laneWidth = 2f;
//    public float obstacleYOffset = 0f; // Adjust this in Inspector if cars float/sink
//    public bool randomRotation = false; // Enable for random car directions

//    private float zSpawn = 0f;
//    private List<GameObject> activeTiles = new List<GameObject>();

//    void Start()
//    {
//        for (int i = 0; i < numberOfTiles; i++)
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//        }
//    }

//    private void Update()
//    {
//        if (playerTransform.position.z - spawnDistance > zSpawn - (numberOfTiles * tileLength))
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//            DeleteOldTiles();
//        }
//    }

//    public void SpawnTile(int tileIndex)
//    {
//        GameObject newTile = Instantiate(
//            tilePrefabs[tileIndex],
//            new Vector3(0, 0, zSpawn),
//            Quaternion.identity
//        );
//        activeTiles.Add(newTile);

//        // Spawn obstacles on this tile
//        SpawnObstaclesOnTile(newTile);

//        zSpawn += tileLength;
//    }

//    private void SpawnObstaclesOnTile(GameObject tile)
//    {
//        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

//        for (int i = 0; i < obstaclesPerTile; i++)
//        {
//            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

//            // Pick random lane (0=left, 1=center, 2=right)
//            int lane = Random.Range(0, 3);
//            float xPos = (lane - 1) * laneWidth;

//            // Random position along the tile
//            float zPos = Random.Range(0.2f, tileLength - 0.2f);

//            // Calculate spawn position with adjustable Y offset
//            Vector3 spawnPos = tile.transform.position + new Vector3(xPos, obstacleYOffset, zPos);

//            // Set rotation - either forward or random
//            Quaternion spawnRotation;
//            if (randomRotation)
//            {
//                // Random 0° or 180° rotation (facing forward or backward)
//                spawnRotation = Quaternion.Euler(0, Random.Range(0, 2) * 180, 0);
//            }
//            else
//            {
//                // All cars face forward
//                spawnRotation = Quaternion.Euler(0, 0, 0);
//            }

//            GameObject obs = Instantiate(obstaclePrefab, spawnPos, spawnRotation, tile.transform);
//            obs.tag = "Obstacle";

//            Debug.Log($"Spawned {obstaclePrefab.name} at {spawnPos} with rotation {spawnRotation.eulerAngles}");
//        }
//    }

//    private void DeleteOldTiles()
//    {
//        if (activeTiles.Count > 0 && activeTiles[0].transform.position.z < playerTransform.position.z - 20f)
//        {
//            Destroy(activeTiles[0]);
//            activeTiles.RemoveAt(0);
//        }
//    }
//}

//using UnityEngine;
//using System.Collections.Generic;

//public class TileManager : MonoBehaviour
//{
//    [Header("Tile Settings")]
//    public GameObject[] tilePrefabs;
//    public float tileLength = 1.983f;
//    public int numberOfTiles = 5;

//    [Header("Player Reference")]
//    public Transform playerTransform;

//    [Header("Spawn Settings")]
//    public float spawnDistance = 35f;

//    [Header("Obstacle Settings")]
//    public GameObject[] obstaclePrefabs;
//    public int obstaclesPerTile = 2;
//    public float laneWidth = 2f;

//    private float zSpawn = 0f;
//    private List<GameObject> activeTiles = new List<GameObject>();

//    void Start()
//    {
//        for (int i = 0; i < numberOfTiles; i++)
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//        }
//    }

//    private void Update()
//    {
//        if (playerTransform.position.z - spawnDistance > zSpawn - (numberOfTiles * tileLength))
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//            DeleteOldTiles();
//        }
//    }

//    public void SpawnTile(int tileIndex)
//    {
//        GameObject newTile = Instantiate(
//            tilePrefabs[tileIndex],
//            new Vector3(0, 0, zSpawn),
//            Quaternion.identity
//        );
//        activeTiles.Add(newTile);

//        // Spawn obstacles on this tile
//        SpawnObstaclesOnTile(newTile);

//        zSpawn += tileLength;
//    }

//    private void SpawnObstaclesOnTile(GameObject tile)
//    {
//        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

//        for (int i = 0; i < obstaclesPerTile; i++)
//        {
//            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

//            // Pick random lane (0=left, 1=center, 2=right)
//            int lane = Random.Range(0, 3);
//            float xPos = (lane - 1) * laneWidth;

//            // Random position along the tile
//            float zPos = Random.Range(0.2f, tileLength - 0.2f);

//            // Spawn relative to tile position
//            Vector3 spawnPos = tile.transform.position + new Vector3(xPos, 0.5f, zPos);

//            GameObject obs = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, tile.transform);
//            obs.tag = "Obstacle";

//            Debug.Log($"Spawned {obstaclePrefab.name} at {spawnPos}");
//        }
//    }

//    private void DeleteOldTiles()
//    {
//        if (activeTiles.Count > 0 && activeTiles[0].transform.position.z < playerTransform.position.z - 20f)
//        {
//            Destroy(activeTiles[0]);
//            activeTiles.RemoveAt(0);
//        }
//    }
//}


//work
//using UnityEngine;
//using System.Collections.Generic;

//public class TileManager : MonoBehaviour
//{
//    [Header("Tile Settings")]
//    public GameObject[] tilePrefabs;
//    public float tileLength = 1.983f;
//    public int numberOfTiles = 5;

//    [Header("Player Reference")]
//    public Transform playerTransform;

//    [Header("Spawn Settings")]
//    public float spawnDistance = 35f; // Distance ahead of player to spawn new tiles

//    private float zSpawn = 0f;
//    private List<GameObject> activeTiles = new List<GameObject>();

//    void Start()
//    {
//        // Spawn initial tiles
//        for (int i = 0; i < numberOfTiles; i++)
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//        }
//    }

//    private void Update()
//    {
//        // Check if we need to spawn a new tile
//        // When player is within spawnDistance of the last spawned tile
//        if (playerTransform.position.z - spawnDistance > zSpawn - (numberOfTiles * tileLength))
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//            DeleteOldTiles();
//        }
//    }

//    public void SpawnTile(int tileIndex)
//    {
//        // Spawn tile at the correct Z position
//        GameObject newTile = Instantiate(
//            tilePrefabs[tileIndex],
//            new Vector3(0, 0, zSpawn),
//            Quaternion.identity
//        );

//        activeTiles.Add(newTile);
//        zSpawn += tileLength;
//    }

//    private void DeleteOldTiles()
//    {
//        // Remove tiles that are behind the player
//        if (activeTiles.Count > 0 && activeTiles[0].transform.position.z < playerTransform.position.z - 20f)
//        {
//            Destroy(activeTiles[0]);
//            activeTiles.RemoveAt(0);
//        }
//    }
//}


//using UnityEngine;

//public class TileManager : MonoBehaviour
//{
//    public GameObject[] tilePrefabs;
//    public float zSpawn = 0;
//    public float tileLength = 1.983f;
//    public int numberOfTiles = 5;

//    void Start()
//    {
//        //SpawnTile(0);
//        //SpawnTile(1);
//        for (int i = 0; i < numberOfTiles; i++)
//        {
//            SpawnTile(Random.Range(0, tilePrefabs.Length));
//        }

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    public void SpawnTile(int tileIndex)
//    {
//        GameObject go = Instantiate(tilePrefabs[tileIndex], transform.forward * zSpawn, transform.rotation);
//        zSpawn += tileLength;

//    }
//}

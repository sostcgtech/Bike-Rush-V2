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
    public float spawnDistance = 35f; // Distance ahead of player to spawn new tiles

    private float zSpawn = 0f;
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        // Spawn initial tiles
        for (int i = 0; i < numberOfTiles; i++)
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
        }
    }

    private void Update()
    {
        // Check if we need to spawn a new tile
        // When player is within spawnDistance of the last spawned tile
        if (playerTransform.position.z - spawnDistance > zSpawn - (numberOfTiles * tileLength))
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
            DeleteOldTiles();
        }
    }

    public void SpawnTile(int tileIndex)
    {
        // Spawn tile at the correct Z position
        GameObject newTile = Instantiate(
            tilePrefabs[tileIndex],
            new Vector3(0, 0, zSpawn),
            Quaternion.identity
        );

        activeTiles.Add(newTile);
        zSpawn += tileLength;
    }

    private void DeleteOldTiles()
    {
        // Remove tiles that are behind the player
        if (activeTiles.Count > 0 && activeTiles[0].transform.position.z < playerTransform.position.z - 20f)
        {
            Destroy(activeTiles[0]);
            activeTiles.RemoveAt(0);
        }
    }
}


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

using UnityEngine;

public class RoadT : MonoBehaviour
{
    private InfiniteRoadSpawner spawnerRef;
    private bool triggered = false;

    public void Initialize(InfiniteRoadSpawner spawner)
    {
        spawnerRef = spawner;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if motorcycle entered (you can change the tag to match your motorcycle)
        if (!triggered && (other.CompareTag("Player") || other.CompareTag("Motorcycle")))
        {
            triggered = true;
            if (spawnerRef != null)
            {
                spawnerRef.OnRoadEntered();
            }
        }
    }
}
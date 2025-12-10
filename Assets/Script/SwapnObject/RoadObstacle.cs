using UnityEngine;

/// <summary>
/// Road Obstacle Controller - Handles individual obstacle behavior.
/// Attach to obstacle prefabs (roadblocks, cones, barriers, etc.)
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoadObstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Does this obstacle cause player to crash")]
    public bool causesCrash = true;
    [Tooltip("Should obstacle be destroyed/hidden on hit")]
    public bool destroyOnHit = false;

    [Header("Effects")]
    [Tooltip("Particle effect on collision")]
    public GameObject hitEffect;
    [Tooltip("Sound on collision")]
    public AudioClip hitSound;

    [Header("Movement (Optional)")]
    [Tooltip("Does this obstacle move")]
    public bool isMoving = false;
    public float moveSpeed = 0f;

    void Update()
    {
        if (isMoving && moveSpeed > 0f)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    void HandleCollision(GameObject other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Motorcycle")) return;

        // Play effects
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        // Notify game manager
        if (causesCrash && GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCrashed();
        }

        // Hide obstacle if set
        if (destroyOnHit)
        {
            gameObject.SetActive(false);
        }
    }
}

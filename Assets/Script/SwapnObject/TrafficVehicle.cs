using UnityEngine;

/// <summary>
/// Traffic Vehicle Controller - Handles individual vehicle movement and behavior.
/// Attach to traffic vehicle prefabs.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrafficVehicle : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public int currentLane = 0;

    [Header("Collision")]
    [Tooltip("Should this vehicle trigger player crash on collision")]
    public bool causesCrash = true;

    // Components
    private Rigidbody rb;
    private bool hasRigidbody;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hasRigidbody = (rb != null);

        if (hasRigidbody)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                            RigidbodyConstraints.FreezeRotationZ | 
                            RigidbodyConstraints.FreezePositionY;
        }
    }

    /// <summary>
    /// Initialize vehicle with speed and lane assignment
    /// </summary>
    public void Setup(float vehicleSpeed, int lane)
    {
        speed = vehicleSpeed;
        currentLane = lane;
    }

    void Update()
    {
        if (!hasRigidbody)
        {
            // Transform-based movement
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        }
    }

    void FixedUpdate()
    {
        if (hasRigidbody)
        {
            // Physics-based movement
            rb.linearVelocity = transform.forward * speed;
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
        if (!causesCrash) return;

        if (other.CompareTag("Player") || other.CompareTag("Motorcycle"))
        {
            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerCrashed();
            }
        }
    }

    void OnEnable()
    {
        // Reset velocity when reactivated
        if (hasRigidbody && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}

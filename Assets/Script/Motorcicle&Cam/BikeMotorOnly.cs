using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BikeMotorOnly : MonoBehaviour
{
    [Header("Speed")]
    public float baseSpeed = 8f;
    public float speedIncreasePerSecond = 0.5f;
    public float maxSpeed = 30f;

    [Header("Lanes")]
    public int laneCount = 3;
    public float laneWidth = 2f;
    public float laneChangeSpeed = 8f;

    [Header("Wheels & Visuals")]
    public Transform frontWheelVisual;
    public Transform backWheelVisual;
    public float wheelCircumference = 0.7f;
    public float transitionTiltAngle = 15f;
    public float tiltSpeed = 8f;

    [Header("Screen Shake")]
    public float shakeIntensity = 0.15f;
    public float shakeDuration = 0.2f;

    [Header("Input Settings")]
    public float swipeThreshold = 50f;

    Rigidbody rb;
    int currentLane = 1;
    Vector3 targetPosition;
    float speed;

    private float[] laneCenters;
    private Vector2 touchStartPos;
    private bool isDragging = false;
    private bool hasProcessedSwipe = false;

    private float currentTilt = 0f;
    private float targetTilt = 0f;
    private bool isChangingLane = false;
    private float shakeTimer = 0f;
    private Vector3 bikeOriginalPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;

        speed = baseSpeed;

        laneCenters = new float[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            laneCenters[i] = (i - 1) * laneWidth;
        }

        currentLane = 1;
        UpdateTargetImmediate();
    }

    void Start()
    {
        laneCount = Mathf.Max(1, laneCount);
    }

    void Update()
    {
        speed += speedIncreasePerSecond * Time.deltaTime;
        speed = Mathf.Min(speed, maxSpeed);

        HandleInput();

        bikeOriginalPosition = transform.position;

        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);

        float laneCenter = laneCenters[currentLane];
        float halfWidth = laneWidth / 2f;
        pos.x = Mathf.Clamp(pos.x, laneCenter - halfWidth, laneCenter + halfWidth);

        if (isChangingLane && Mathf.Abs(pos.x - targetPosition.x) < 0.05f)
        {
            isChangingLane = false;
            targetTilt = 0f;
        }

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, currentTilt);

        ApplyShake();
        RotateWheels();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isDragging = true;
            hasProcessedSwipe = false;
        }

        if (Input.GetMouseButton(0) && isDragging && !hasProcessedSwipe)
        {
            Vector2 currentPos = Input.mousePosition;
            float swipeDistance = currentPos.x - touchStartPos.x;

            if (Mathf.Abs(swipeDistance) > swipeThreshold)
            {
                if (swipeDistance > 0)
                    MoveRight();
                else
                    MoveLeft();

                hasProcessedSwipe = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            hasProcessedSwipe = false;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isDragging = true;
                hasProcessedSwipe = false;
            }
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                     && isDragging && !hasProcessedSwipe)
            {
                Vector2 currentPos = touch.position;
                float swipeDistance = currentPos.x - touchStartPos.x;

                if (Mathf.Abs(swipeDistance) > swipeThreshold)
                {
                    if (swipeDistance > 0)
                        MoveRight();
                    else
                        MoveLeft();

                    hasProcessedSwipe = true;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
                hasProcessedSwipe = false;
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.linearVelocity;
        vel.z = speed;
        rb.linearVelocity = vel;
    }

    void RotateWheels()
    {
        float rotationAmount = (speed / wheelCircumference) * 360f * Time.deltaTime;
        if (frontWheelVisual != null)
            frontWheelVisual.Rotate(Vector3.right, rotationAmount);
        if (backWheelVisual != null)
            backWheelVisual.Rotate(Vector3.right, rotationAmount);
    }

    public void MoveLeft()
    {
        if (currentLane > 0)
        {
            currentLane--;
            UpdateTarget();
            targetTilt = transitionTiltAngle;
            isChangingLane = true;
            TriggerShake();
            Debug.Log("Moved LEFT to " + GetLaneInfo());
        }
    }

    public void MoveRight()
    {
        if (currentLane < laneCount - 1)
        {
            currentLane++;
            UpdateTarget();
            targetTilt = -transitionTiltAngle;
            isChangingLane = true;
            TriggerShake();
            Debug.Log("Moved RIGHT to " + GetLaneInfo());
        }
    }

    void UpdateTarget()
    {
        float x = laneCenters[currentLane];
        targetPosition = new Vector3(x, transform.position.y, transform.position.z);
    }

    void UpdateTargetImmediate()
    {
        float x = laneCenters[currentLane];
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
        targetPosition = transform.position;
        targetTilt = 0f;
        currentTilt = 0f;
    }

    void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    void ApplyShake()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = bikeOriginalPosition + new Vector3(shakeX, shakeY, 0f);
        }
        else if (shakeTimer < 0f)
        {
            shakeTimer = 0f;
            transform.position = bikeOriginalPosition;
        }
    }

    string GetLaneInfo()
    {
        float center = laneCenters[currentLane];
        float leftBound = center - (laneWidth / 2f);
        float rightBound = center + (laneWidth / 2f);
        string laneName = currentLane == 0 ? "LEFT" : (currentLane == 1 ? "CENTER" : "RIGHT");
        return string.Format("{0} Lane (Center: X={1}, Boundaries: {2} to {3})",
            laneName, center, leftBound, rightBound);
    }

    public float GetCurrentSpeed() { return speed; }
    public int GetCurrentLane() { return currentLane; }
    public float GetCurrentLaneCenter() { return laneCenters[currentLane]; }
}

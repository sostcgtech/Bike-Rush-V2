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
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class BikeMotorOnly : MonoBehaviour
//{
//    [Header("Speed")]
//    public float baseSpeed = 8f;                 // starting forward speed (m/s)
//    public float speedIncreasePerSecond = 0.5f;  // how fast speed ramps
//    public float maxSpeed = 30f;                 // clamp top speed

//    [Header("Lanes")]
//    public int laneCount = 3;                    // 3 lanes (0=left, 1=center, 2=right)
//    public float laneWidth = 2f;                 // width of each lane (distance between centers)
//    public float laneChangeSpeed = 8f;           // how fast bike shifts lanes

//    [Header("Wheels & Visuals")]
//    public Transform frontWheelVisual;
//    public Transform backWheelVisual;
//    public float wheelCircumference = 0.7f;      // adjust to match wheel mesh size
//    public float transitionTiltAngle = 15f;      // temporary tilt during lane change
//    public float tiltSpeed = 8f;                 // how fast tilt returns to upright

//    [Header("Screen Shake")]
//    public float shakeIntensity = 0.15f;         // how much to shake
//    public float shakeDuration = 0.2f;           // how long shake lasts

//    [Header("Input Settings")]
//    public float swipeThreshold = 50f;           // minimum swipe distance in pixels

//    Rigidbody rb;
//    int currentLane = 1;                         // center lane = 1 (0=left, 1=center, 2=right)
//    Vector3 targetPosition;
//    float speed;

//    // Lane center positions
//    // Lane 0 (Left): X = -2
//    // Lane 1 (Center): X = 0
//    // Lane 2 (Right): X = 2
//    private float[] laneCenters;

//    // Touch/Mouse input tracking
//    private Vector2 touchStartPos;
//    private bool isDragging = false;
//    private bool hasProcessedSwipe = false;

//    // Tilt and shake
//    private float currentTilt = 0f;              // current Z rotation tilt
//    private float targetTilt = 0f;               // target tilt (0 = upright)
//    private bool isChangingLane = false;         // flag for lane transition
//    private float shakeTimer = 0f;               // countdown for shake effect
//    private Vector3 bikeOriginalPosition;        // to restore after shake

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.constraints = RigidbodyConstraints.FreezeRotationX |
//                        RigidbodyConstraints.FreezeRotationY |
//                        RigidbodyConstraints.FreezeRotationZ;

//        speed = baseSpeed;

//        // Initialize lane centers
//        // Lane 0 (Left) = -2, Lane 1 (Center) = 0, Lane 2 (Right) = 2
//        laneCenters = new float[laneCount];
//        for (int i = 0; i < laneCount; i++)
//        {
//            laneCenters[i] = (i - 1) * laneWidth; // -2, 0, 2
//        }

//        currentLane = 1;  // start at center
//        UpdateTargetImmediate();
//    }

//    void Start()
//    {
//        laneCount = Mathf.Max(1, laneCount);
//    }

//    void Update()
//    {
//        // Increase forward speed over time
//        speed += speedIncreasePerSecond * Time.deltaTime;
//        speed = Mathf.Min(speed, maxSpeed);

//        // Handle input
//        HandleInput();

//        // Store position before shake
//        bikeOriginalPosition = transform.position;

//        // Smooth lateral movement towards target x with clamping
//        Vector3 pos = transform.position;
//        pos.x = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);

//        // Clamp position to current lane boundaries
//        float laneCenter = laneCenters[currentLane];
//        float halfWidth = laneWidth / 2f;
//        pos.x = Mathf.Clamp(pos.x, laneCenter - halfWidth, laneCenter + halfWidth);

//        // Check if bike reached target (lane change complete)
//        if (isChangingLane && Mathf.Abs(pos.x - targetPosition.x) < 0.05f)
//        {
//            isChangingLane = false;
//            targetTilt = 0f; // return to upright
//        }

//        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

//        // Smooth tilt transition (always returns to 0 = upright)
//        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
//        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, currentTilt);

//        // Apply screen shake effect
//        ApplyShake();

//        RotateWheels();
//    }

//    void HandleInput()
//    {
//        // Keyboard input (Editor/Desktop)
//        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
//        {
//            MoveLeft();
//        }
//        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
//        {
//            MoveRight();
//        }

//        // Mouse input (Desktop/Editor)
//        if (Input.GetMouseButtonDown(0))
//        {
//            touchStartPos = Input.mousePosition;
//            isDragging = true;
//            hasProcessedSwipe = false;
//        }

//        if (Input.GetMouseButton(0) && isDragging && !hasProcessedSwipe)
//        {
//            Vector2 currentPos = Input.mousePosition;
//            float swipeDistance = currentPos.x - touchStartPos.x;

//            if (Mathf.Abs(swipeDistance) > swipeThreshold)
//            {
//                if (swipeDistance > 0)
//                    MoveRight();
//                else
//                    MoveLeft();

//                hasProcessedSwipe = true;
//            }
//        }

//        if (Input.GetMouseButtonUp(0))
//        {
//            isDragging = false;
//            hasProcessedSwipe = false;
//        }

//        // Touch input (Mobile)
//        if (Input.touchCount > 0)
//        {
//            Touch touch = Input.GetTouch(0);

//            if (touch.phase == TouchPhase.Began)
//            {
//                touchStartPos = touch.position;
//                isDragging = true;
//                hasProcessedSwipe = false;
//            }
//            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
//                     && isDragging && !hasProcessedSwipe)
//            {
//                Vector2 currentPos = touch.position;
//                float swipeDistance = currentPos.x - touchStartPos.x;

//                if (Mathf.Abs(swipeDistance) > swipeThreshold)
//                {
//                    if (swipeDistance > 0)
//                        MoveRight();
//                    else
//                        MoveLeft();

//                    hasProcessedSwipe = true;
//                }
//            }
//            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//            {
//                isDragging = false;
//                hasProcessedSwipe = false;
//            }
//        }
//    }

//    void FixedUpdate()
//    {
//        // Set forward velocity while preserving existing vertical velocity (gravity)
//        Vector3 vel = rb.linearVelocity;
//        vel.z = speed;
//        rb.linearVelocity = vel;
//    }

//    void RotateWheels()
//    {
//        float rotationAmount = (speed / wheelCircumference) * 360f * Time.deltaTime;
//        if (frontWheelVisual != null)
//            frontWheelVisual.Rotate(Vector3.right, rotationAmount);
//        if (backWheelVisual != null)
//            backWheelVisual.Rotate(Vector3.right, rotationAmount);
//    }

//    public void MoveLeft()
//    {
//        // Move to left lane if not already at leftmost
//        if (currentLane > 0)
//        {
//            currentLane--;
//            UpdateTarget();

//            // Apply temporary tilt to the RIGHT (leaning into the turn)
//            targetTilt = transitionTiltAngle;
//            isChangingLane = true;

//            // Trigger shake effect
//            TriggerShake();

//            string laneInfo = GetLaneInfo();
//            Debug.Log("Moved LEFT to " + laneInfo);
//        }
//        else
//        {
//            Debug.Log("Already at leftmost lane! (X = -2, boundaries: -3 to -1)");
//        }
//    }

//    public void MoveRight()
//    {
//        // Move to right lane if not already at rightmost
//        if (currentLane < laneCount - 1)
//        {
//            currentLane++;
//            UpdateTarget();

//            // Apply temporary tilt to the LEFT (leaning into the turn)
//            targetTilt = -transitionTiltAngle;
//            isChangingLane = true;

//            // Trigger shake effect
//            TriggerShake();

//            string laneInfo = GetLaneInfo();
//            Debug.Log("Moved RIGHT to " + laneInfo);
//        }
//        else
//        {
//            Debug.Log("Already at rightmost lane! (X = 2, boundaries: 1 to 3)");
//        }
//    }

//    void UpdateTarget()
//    {
//        float x = laneCenters[currentLane];
//        targetPosition = new Vector3(x, transform.position.y, transform.position.z);
//    }

//    void UpdateTargetImmediate()
//    {
//        float x = laneCenters[currentLane];
//        transform.position = new Vector3(x, transform.position.y, transform.position.z);
//        targetPosition = transform.position;
//        targetTilt = 0f; // start upright
//        currentTilt = 0f;
//    }

//    void TriggerShake()
//    {
//        shakeTimer = shakeDuration;
//    }

//    void ApplyShake()
//    {
//        if (shakeTimer > 0f)
//        {
//            shakeTimer -= Time.deltaTime;

//            // Random shake offset
//            float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
//            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);

//            transform.position = bikeOriginalPosition + new Vector3(shakeX, shakeY, 0f);
//        }
//        else if (shakeTimer < 0f)
//        {
//            shakeTimer = 0f;
//            // Ensure position is restored
//            transform.position = bikeOriginalPosition;
//        }
//    }

//    string GetLaneInfo()
//    {
//        float center = laneCenters[currentLane];
//        float leftBound = center - (laneWidth / 2f);
//        float rightBound = center + (laneWidth / 2f);

//        string laneName = currentLane == 0 ? "LEFT" : (currentLane == 1 ? "CENTER" : "RIGHT");
//        return string.Format("{0} Lane (Center: X={1}, Boundaries: {2} to {3})",
//            laneName, center, leftBound, rightBound);
//    }

//    // Public methods for external scripts
//    public float GetCurrentSpeed()
//    {
//        return speed;
//    }

//    public int GetCurrentLane()
//    {
//        return currentLane;
//    }

//    public float GetCurrentLaneCenter()
//    {
//        return laneCenters[currentLane];
//    }
//}




//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class BikeMotorOnly : MonoBehaviour
//{
//    [Header("Speed")]
//    public float baseSpeed = 8f;                 // starting forward speed (m/s)
//    public float speedIncreasePerSecond = 0.5f;  // how fast speed ramps
//    public float maxSpeed = 30f;                 // clamp top speed

//    [Header("Lanes")]
//    public int laneCount = 3;                    // 3 lanes (0=left, 1=center, 2=right)
//    public float laneWidth = 2f;                 // width of each lane (distance between centers)
//    public float laneChangeSpeed = 8f;           // how fast bike shifts lanes

//    [Header("Wheels & Visuals")]
//    public Transform frontWheelVisual;
//    public Transform backWheelVisual;
//    public float wheelCircumference = 0.7f;      // adjust to match wheel mesh size
//    public float tiltAngle = 10f;                // visual tilt on lane change

//    [Header("Input Settings")]
//    public float swipeThreshold = 50f;           // minimum swipe distance in pixels

//    Rigidbody rb;
//    int currentLane = 1;                         // center lane = 1 (0=left, 1=center, 2=right)
//    Vector3 targetPosition;
//    float speed;

//    // Lane center positions
//    // Lane 0 (Left): X = -2
//    // Lane 1 (Center): X = 0
//    // Lane 2 (Right): X = 2
//    private float[] laneCenters;

//    // Touch/Mouse input tracking
//    private Vector2 touchStartPos;
//    private bool isDragging = false;
//    private bool hasProcessedSwipe = false;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.constraints = RigidbodyConstraints.FreezeRotationX |
//                        RigidbodyConstraints.FreezeRotationY |
//                        RigidbodyConstraints.FreezeRotationZ;

//        speed = baseSpeed;

//        // Initialize lane centers
//        // Lane 0 (Left) = -2, Lane 1 (Center) = 0, Lane 2 (Right) = 2
//        laneCenters = new float[laneCount];
//        for (int i = 0; i < laneCount; i++)
//        {
//            laneCenters[i] = (i - 1) * laneWidth; // -2, 0, 2
//        }

//        currentLane = 1;  // start at center
//        UpdateTargetImmediate();
//    }

//    void Start()
//    {
//        laneCount = Mathf.Max(1, laneCount);
//    }

//    void Update()
//    {
//        // Increase forward speed over time
//        speed += speedIncreasePerSecond * Time.deltaTime;
//        speed = Mathf.Min(speed, maxSpeed);

//        // Handle input
//        HandleInput();

//        // Smooth lateral movement towards target x with clamping
//        Vector3 pos = transform.position;
//        pos.x = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);

//        // Clamp position to current lane boundaries
//        float laneCenter = laneCenters[currentLane];
//        float halfWidth = laneWidth / 2f;
//        pos.x = Mathf.Clamp(pos.x, laneCenter - halfWidth, laneCenter + halfWidth);

//        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

//        // Visual tilt based on lane offset
//        float desiredTilt = (currentLane - 1) * -tiltAngle; // -1 for left, 0 for center, +1 for right
//        float zAngle = Mathf.LerpAngle(transform.eulerAngles.z, desiredTilt, Time.deltaTime * 6f);
//        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zAngle);

//        RotateWheels();
//    }

//    void HandleInput()
//    {
//        // Keyboard input (Editor/Desktop)
//        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
//        {
//            MoveLeft();
//        }
//        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
//        {
//            MoveRight();
//        }

//        // Mouse input (Desktop/Editor)
//        if (Input.GetMouseButtonDown(0))
//        {
//            touchStartPos = Input.mousePosition;
//            isDragging = true;
//            hasProcessedSwipe = false;
//        }

//        if (Input.GetMouseButton(0) && isDragging && !hasProcessedSwipe)
//        {
//            Vector2 currentPos = Input.mousePosition;
//            float swipeDistance = currentPos.x - touchStartPos.x;

//            if (Mathf.Abs(swipeDistance) > swipeThreshold)
//            {
//                if (swipeDistance > 0)
//                    MoveRight();
//                else
//                    MoveLeft();

//                hasProcessedSwipe = true;
//            }
//        }

//        if (Input.GetMouseButtonUp(0))
//        {
//            isDragging = false;
//            hasProcessedSwipe = false;
//        }

//        // Touch input (Mobile)
//        if (Input.touchCount > 0)
//        {
//            Touch touch = Input.GetTouch(0);

//            if (touch.phase == TouchPhase.Began)
//            {
//                touchStartPos = touch.position;
//                isDragging = true;
//                hasProcessedSwipe = false;
//            }
//            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
//                     && isDragging && !hasProcessedSwipe)
//            {
//                Vector2 currentPos = touch.position;
//                float swipeDistance = currentPos.x - touchStartPos.x;

//                if (Mathf.Abs(swipeDistance) > swipeThreshold)
//                {
//                    if (swipeDistance > 0)
//                        MoveRight();
//                    else
//                        MoveLeft();

//                    hasProcessedSwipe = true;
//                }
//            }
//            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//            {
//                isDragging = false;
//                hasProcessedSwipe = false;
//            }
//        }
//    }

//    void FixedUpdate()
//    {
//        // Set forward velocity while preserving existing vertical velocity (gravity)
//        Vector3 vel = rb.linearVelocity;
//        vel.z = speed;
//        rb.linearVelocity = vel;
//    }

//    void RotateWheels()
//    {
//        float rotationAmount = (speed / wheelCircumference) * 360f * Time.deltaTime;
//        if (frontWheelVisual != null)
//            frontWheelVisual.Rotate(Vector3.right, rotationAmount);
//        if (backWheelVisual != null)
//            backWheelVisual.Rotate(Vector3.right, rotationAmount);
//    }

//    public void MoveLeft()
//    {
//        // Move to left lane if not already at leftmost
//        if (currentLane > 0)
//        {
//            currentLane--;
//            UpdateTarget();
//            string laneInfo = GetLaneInfo();
//            Debug.Log("Moved LEFT to " + laneInfo);
//        }
//        else
//        {
//            Debug.Log("Already at leftmost lane! (X = -2, boundaries: -3 to -1)");
//        }
//    }

//    public void MoveRight()
//    {
//        // Move to right lane if not already at rightmost
//        if (currentLane < laneCount - 1)
//        {
//            currentLane++;
//            UpdateTarget();
//            string laneInfo = GetLaneInfo();
//            Debug.Log("Moved RIGHT to " + laneInfo);
//        }
//        else
//        {
//            Debug.Log("Already at rightmost lane! (X = 2, boundaries: 1 to 3)");
//        }
//    }

//    void UpdateTarget()
//    {
//        float x = laneCenters[currentLane];
//        targetPosition = new Vector3(x, transform.position.y, transform.position.z);
//    }

//    void UpdateTargetImmediate()
//    {
//        float x = laneCenters[currentLane];
//        transform.position = new Vector3(x, transform.position.y, transform.position.z);
//        targetPosition = transform.position;
//    }

//    string GetLaneInfo()
//    {
//        float center = laneCenters[currentLane];
//        float leftBound = center - (laneWidth / 2f);
//        float rightBound = center + (laneWidth / 2f);

//        string laneName = currentLane == 0 ? "LEFT" : (currentLane == 1 ? "CENTER" : "RIGHT");
//        return string.Format("{0} Lane (Center: X={1}, Boundaries: {2} to {3})",
//            laneName, center, leftBound, rightBound);
//    }

//    // Public methods for external scripts
//    public float GetCurrentSpeed()
//    {
//        return speed;
//    }

//    public int GetCurrentLane()
//    {
//        return currentLane;
//    }

//    public float GetCurrentLaneCenter()
//    {
//        return laneCenters[currentLane];
//    }
//}




//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class BikeMotorOnly : MonoBehaviour
//{
//    [Header("Speed")]
//    public float baseSpeed = 8f;                 // starting forward speed (m/s)
//    public float speedIncreasePerSecond = 0.5f;  // how fast speed ramps
//    public float maxSpeed = 30f;                 // clamp top speed

//    [Header("Lanes")]
//    public int laneCount = 3;                    // 3 lanes
//    public float laneDistance = 1.8f;            // distance between lanes (portrait friendly)
//    public float laneChangeSpeed = 8f;           // how fast bike shifts lanes

//    [Header("Wheels & Visuals")]
//    public Transform frontWheelVisual;
//    public Transform backWheelVisual;
//    public float wheelCircumference = 0.7f;      // adjust to match wheel mesh size
//    public float tiltAngle = 10f;                // visual tilt on lane change

//    [Header("Input Settings")]
//    public float swipeThreshold = 50f;           // minimum swipe distance in pixels

//    Rigidbody rb;
//    int currentLane = 1;                         // center = 1 (0 left, 2 right)
//    Vector3 targetPosition;
//    float speed;

//    // Touch/Mouse input tracking
//    private Vector2 touchStartPos;
//    private bool isDragging = false;
//    private bool hasProcessedSwipe = false;  // prevents multiple lane changes per swipe

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.constraints = RigidbodyConstraints.FreezeRotationX |
//                        RigidbodyConstraints.FreezeRotationY |
//                        RigidbodyConstraints.FreezeRotationZ;
//        // Added FreezeRotationZ - remove if you want the tilt on Z axis

//        speed = baseSpeed;
//        UpdateTargetImmediate();
//    }

//    void Start()
//    {
//        laneCount = Mathf.Max(1, laneCount);
//    }

//    void Update()
//    {
//        // Increase forward speed over time
//        speed += speedIncreasePerSecond * Time.deltaTime;
//        speed = Mathf.Min(speed, maxSpeed);

//        // Handle input
//        HandleInput();

//        // Smooth lateral movement towards target x
//        Vector3 pos = transform.position;
//        pos.x = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);
//        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

//        // Visual tilt based on lane offset (-1,0,+1)
//        float desiredTilt = (currentLane - (laneCount - 1) * 0.5f) * -tiltAngle;
//        float zAngle = Mathf.LerpAngle(transform.eulerAngles.z, desiredTilt, Time.deltaTime * 6f);
//        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zAngle);

//        RotateWheels();
//    }

//    void HandleInput()
//    {
//        // Keyboard input (Editor/Desktop)
//        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
//        {
//            MoveLeft();
//        }
//        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
//        {
//            MoveRight();
//        }

//        // Mouse input (Desktop/Editor)
//        if (Input.GetMouseButtonDown(0))
//        {
//            touchStartPos = Input.mousePosition;
//            isDragging = true;
//            hasProcessedSwipe = false;  // reset swipe flag
//        }

//        if (Input.GetMouseButton(0) && isDragging && !hasProcessedSwipe)
//        {
//            Vector2 currentPos = Input.mousePosition;
//            float swipeDistance = currentPos.x - touchStartPos.x;

//            if (Mathf.Abs(swipeDistance) > swipeThreshold)
//            {
//                if (swipeDistance > 0)
//                    MoveRight();
//                else
//                    MoveLeft();

//                hasProcessedSwipe = true;  // mark as processed (one lane change per swipe)
//            }
//        }

//        if (Input.GetMouseButtonUp(0))
//        {
//            isDragging = false;
//            hasProcessedSwipe = false;
//        }

//        // Touch input (Mobile)
//        if (Input.touchCount > 0)
//        {
//            Touch touch = Input.GetTouch(0);

//            if (touch.phase == TouchPhase.Began)
//            {
//                touchStartPos = touch.position;
//                isDragging = true;
//                hasProcessedSwipe = false;  // reset swipe flag
//            }
//            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
//                     && isDragging && !hasProcessedSwipe)
//            {
//                Vector2 currentPos = touch.position;
//                float swipeDistance = currentPos.x - touchStartPos.x;

//                if (Mathf.Abs(swipeDistance) > swipeThreshold)
//                {
//                    if (swipeDistance > 0)
//                        MoveRight();
//                    else
//                        MoveLeft();

//                    hasProcessedSwipe = true;  // mark as processed (one lane change per swipe)
//                }
//            }
//            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//            {
//                isDragging = false;
//                hasProcessedSwipe = false;
//            }
//        }
//    }

//    void FixedUpdate()
//    {
//        // Set forward velocity while preserving existing vertical velocity (gravity)
//        Vector3 vel = rb.linearVelocity;
//        vel.z = speed;
//        rb.linearVelocity = vel;
//    }

//    void RotateWheels()
//    {
//        float rotationAmount = (speed / wheelCircumference) * 360f * Time.deltaTime;
//        if (frontWheelVisual != null)
//            frontWheelVisual.Rotate(Vector3.right, rotationAmount);
//        if (backWheelVisual != null)
//            backWheelVisual.Rotate(Vector3.right, rotationAmount);
//    }

//    public void MoveLeft()
//    {
//        if (currentLane > 0)
//        {
//            currentLane--;
//            UpdateTarget();
//            Debug.Log("Moved to lane: " + currentLane);
//        }
//    }

//    public void MoveRight()
//    {
//        if (currentLane < laneCount - 1)
//        {
//            currentLane++;
//            UpdateTarget();
//            Debug.Log("Moved to lane: " + currentLane);
//        }
//    }

//    void UpdateTarget()
//    {
//        float centerOffset = -(laneCount - 1) * 0.5f * laneDistance;
//        float x = centerOffset + currentLane * laneDistance;
//        targetPosition = new Vector3(x, transform.position.y, transform.position.z);
//    }

//    void UpdateTargetImmediate()
//    {
//        float centerOffset = -(laneCount - 1) * 0.5f * laneDistance;
//        float x = centerOffset + currentLane * laneDistance;
//        transform.position = new Vector3(x, transform.position.y, transform.position.z);
//        targetPosition = transform.position;
//    }

//    // Public method to get current speed (useful for other scripts)
//    public float GetCurrentSpeed()
//    {
//        return speed;
//    }

//    // Public method to get current lane (useful for other scripts)
//    public int GetCurrentLane()
//    {
//        return currentLane;
//    }
//}
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class BikeMotorOnly : MonoBehaviour
//{
//    [Header("Speed")]
//    public float baseSpeed = 8f;                 // starting forward speed (m/s)
//    public float speedIncreasePerSecond = 0.5f;  // how fast speed ramps
//    public float maxSpeed = 30f;                 // clamp top speed

//    [Header("Lanes")]
//    public int laneCount = 3;                    // 3 lanes
//    public float laneDistance = 1.8f;            // distance between lanes (portrait friendly)
//    public float laneChangeSpeed = 8f;           // how fast bike shifts lanes

//    [Header("Wheels & Visuals")]
//    public Transform frontWheelVisual;
//    public Transform backWheelVisual;
//    public float wheelCircumference = 0.7f;      // adjust to match wheel mesh size
//    public float tiltAngle = 10f;                // visual tilt on lane change

//    [Header("Input Settings")]
//    public float swipeThreshold = 50f;           // minimum swipe distance in pixels

//    Rigidbody rb;
//    int currentLane = 1;                         // center = 1 (0 left, 2 right)
//    Vector3 targetPosition;
//    float speed;

//    // Touch/Mouse input tracking
//    private Vector2 touchStartPos;
//    private bool isDragging = false;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.constraints = RigidbodyConstraints.FreezeRotationX |
//                        RigidbodyConstraints.FreezeRotationY |
//                        RigidbodyConstraints.FreezeRotationZ;
//        // Added FreezeRotationZ - remove if you want the tilt on Z axis

//        speed = baseSpeed;
//        UpdateTargetImmediate();
//    }

//    void Start()
//    {
//        laneCount = Mathf.Max(1, laneCount);
//    }

//    void Update()
//    {
//        // Increase forward speed over time
//        speed += speedIncreasePerSecond * Time.deltaTime;
//        speed = Mathf.Min(speed, maxSpeed);

//        // Handle input
//        HandleInput();

//        // Smooth lateral movement towards target x
//        Vector3 pos = transform.position;
//        pos.x = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);
//        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

//        // Visual tilt based on lane offset (-1,0,+1)
//        float desiredTilt = (currentLane - (laneCount - 1) * 0.5f) * -tiltAngle;
//        float zAngle = Mathf.LerpAngle(transform.eulerAngles.z, desiredTilt, Time.deltaTime * 6f);
//        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zAngle);

//        RotateWheels();
//    }

//    void HandleInput()
//    {
//        // Keyboard input (Editor/Desktop)
//        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
//        {
//            MoveLeft();
//        }
//        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
//        {
//            MoveRight();
//        }

//        // Mouse input (Desktop/Editor)
//        if (Input.GetMouseButtonDown(0))
//        {
//            touchStartPos = Input.mousePosition;
//            isDragging = true;
//        }

//        if (Input.GetMouseButtonUp(0) && isDragging)
//        {
//            Vector2 touchEndPos = Input.mousePosition;
//            float swipeDistance = touchEndPos.x - touchStartPos.x;

//            if (Mathf.Abs(swipeDistance) > swipeThreshold)
//            {
//                if (swipeDistance > 0)
//                    MoveRight();
//                else
//                    MoveLeft();
//            }

//            isDragging = false;
//        }

//        // Touch input (Mobile)
//        if (Input.touchCount > 0)
//        {
//            Touch touch = Input.GetTouch(0);

//            if (touch.phase == TouchPhase.Began)
//            {
//                touchStartPos = touch.position;
//                isDragging = true;
//            }
//            else if (touch.phase == TouchPhase.Ended && isDragging)
//            {
//                Vector2 touchEndPos = touch.position;
//                float swipeDistance = touchEndPos.x - touchStartPos.x;

//                if (Mathf.Abs(swipeDistance) > swipeThreshold)
//                {
//                    if (swipeDistance > 0)
//                        MoveRight();
//                    else
//                        MoveLeft();
//                }

//                isDragging = false;
//            }
//        }
//    }

//    void FixedUpdate()
//    {
//        // Set forward velocity while preserving existing vertical velocity (gravity)
//        Vector3 vel = rb.linearVelocity;
//        vel.z = speed;
//        rb.linearVelocity = vel;
//    }

//    void RotateWheels()
//    {
//        float rotationAmount = (speed / wheelCircumference) * 360f * Time.deltaTime;
//        if (frontWheelVisual != null)
//            frontWheelVisual.Rotate(Vector3.right, rotationAmount);
//        if (backWheelVisual != null)
//            backWheelVisual.Rotate(Vector3.right, rotationAmount);
//    }

//    public void MoveLeft()
//    {
//        if (currentLane > 0)
//        {
//            currentLane--;
//            UpdateTarget();
//            Debug.Log("Moved to lane: " + currentLane);
//        }
//    }

//    public void MoveRight()
//    {
//        if (currentLane < laneCount - 1)
//        {
//            currentLane++;
//            UpdateTarget();
//            Debug.Log("Moved to lane: " + currentLane);
//        }
//    }

//    void UpdateTarget()
//    {
//        float centerOffset = -(laneCount - 1) * 0.5f * laneDistance;
//        float x = centerOffset + currentLane * laneDistance;
//        targetPosition = new Vector3(x, transform.position.y, transform.position.z);
//    }

//    void UpdateTargetImmediate()
//    {
//        float centerOffset = -(laneCount - 1) * 0.5f * laneDistance;
//        float x = centerOffset + currentLane * laneDistance;
//        transform.position = new Vector3(x, transform.position.y, transform.position.z);
//        targetPosition = transform.position;
//    }

//    // Public method to get current speed (useful for other scripts)
//    public float GetCurrentSpeed()
//    {
//        return speed;
//    }

//    // Public method to get current lane (useful for other scripts)
//    public int GetCurrentLane()
//    {
//        return currentLane;
//    }
//}



//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class BikeMotorOnly : MonoBehaviour
//{
//    [Header("Speed")]
//    public float baseSpeed = 8f;                 // starting forward speed (m/s)
//    public float speedIncreasePerSecond = 0.5f;  // how fast speed ramps
//    public float maxSpeed = 30f;                 // clamp top speed

//    [Header("Lanes")]
//    public int laneCount = 3;                    // 3 lanes
//    public float laneDistance = 1.8f;            // distance between lanes (portrait friendly)
//    public float laneChangeSpeed = 8f;           // how fast bike shifts lanes

//    [Header("Wheels & Visuals")]
//    public Transform frontWheelVisual;
//    public Transform backWheelVisual;
//    public float wheelCircumference = 0.7f;      // adjust to match wheel mesh size
//    public float tiltAngle = 10f;                // visual tilt on lane change

//    Rigidbody rb;
//    int currentLane = 1;                         // center = 1 (0 left, 2 right)
//    Vector3 targetPosition;
//    float speed;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
//        // Allow Z rotation for a slight tilt feel; change if you prefer upright locking
//        speed = baseSpeed;
//        UpdateTargetImmediate();
//    }

//    void Start()
//    {
//        laneCount = Mathf.Max(1, laneCount);
//    }

//    void Update()
//    {
//        // Increase forward speed over time
//        speed += speedIncreasePerSecond * Time.deltaTime;
//        speed = Mathf.Min(speed, maxSpeed);

//        // smooth lateral movement towards target x
//        Vector3 pos = transform.position;
//        pos.x = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);
//        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

//        // visual tilt based on lane offset (-1,0,+1)
//        float desiredTilt = (currentLane - (laneCount - 1) * 0.5f) * -tiltAngle;
//        float zAngle = Mathf.LerpAngle(transform.eulerAngles.z, desiredTilt, Time.deltaTime * 6f);
//        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, zAngle);

//        // debug keys for testing (Editor & device keyboard)
//#if UNITY_EDITOR || UNITY_STANDALONE
//        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveLeft();
//        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveRight();
//        //Debug.Log("Target X = " + targetPosition.x + " Current X = " + transform.position.x);

//#endif

//        RotateWheels();
//    }

//    void FixedUpdate()
//    {
//        // set forward velocity while preserving existing vertical velocity (gravity)
//        Vector3 vel = rb.linearVelocity;
//        vel.z = speed;
//        rb.linearVelocity = vel;
//    }

//    void RotateWheels()
//    {
//        float rotationAmount = (speed / wheelCircumference) * 360f * Time.deltaTime;
//        if (frontWheelVisual != null) frontWheelVisual.Rotate(Vector3.right, rotationAmount);
//        if (backWheelVisual != null) backWheelVisual.Rotate(Vector3.right, rotationAmount);
//    }

//    public void MoveLeft()
//    {
//        currentLane = Mathf.Clamp(currentLane - 1, 0, laneCount - 1);
//        UpdateTarget();
//    }

//    public void MoveRight()
//    {
//        currentLane = Mathf.Clamp(currentLane + 1, 0, laneCount - 1);
//        UpdateTarget();
//    }

//    void UpdateTarget()
//    {
//        float centerOffset = -(laneCount - 1) * 0.5f * laneDistance;
//        float x = centerOffset + currentLane * laneDistance;
//        targetPosition = new Vector3(x, transform.position.y, transform.position.z);
//    }

//    void UpdateTargetImmediate()
//    {
//        float centerOffset = -(laneCount - 1) * 0.5f * laneDistance;
//        float x = centerOffset + currentLane * laneDistance;
//        transform.position = new Vector3(x, transform.position.y, transform.position.z);
//        targetPosition = transform.position;
//    }
//}

using UnityEngine;
using System;

public class SwipeManager : MonoBehaviour
{
    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;

    Vector2 startPos;
    bool couldBeSwipe;
    float minSwipeDistance = 50f; // pixels

    void Update()
    {
#if UNITY_EDITOR
        // mouse as touch for testing
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            couldBeSwipe = true;
        }
        if (Input.GetMouseButtonUp(0) && couldBeSwipe)
        {
            Vector2 end = (Vector2)Input.mousePosition;
            DetectSwipe(end);
            couldBeSwipe = false;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.touches[0];
            if (t.phase == TouchPhase.Began)
            {
                startPos = t.position;
                couldBeSwipe = true;
            }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && couldBeSwipe)
            {
                DetectSwipe(t.position);
                couldBeSwipe = false;
            }
        }
#endif
    }

    void DetectSwipe(Vector2 endPos)
    {
        Vector2 delta = endPos - startPos;
        if (delta.magnitude < minSwipeDistance) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // horizontal swipe
            if (delta.x > 0) OnSwipeRight?.Invoke();
            else OnSwipeLeft?.Invoke();
        }
    }
}

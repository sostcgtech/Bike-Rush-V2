using UnityEngine;

public class UIButtonClick : MonoBehaviour
{
    public void PlaySound()
    {
        UISoundManager.Instance.PlayClick();
    }
}

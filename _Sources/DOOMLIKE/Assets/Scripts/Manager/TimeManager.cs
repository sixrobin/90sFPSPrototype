using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public void Freeze()
    {
        Time.timeScale = 0f;
    }

    public void Unfreeze()
    {
        Time.timeScale = 1f;
    }
}
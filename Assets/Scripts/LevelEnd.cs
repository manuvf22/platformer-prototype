using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.StopLevel();
        UIManager.Instance.ShowLevelEndPanel();
    }
}
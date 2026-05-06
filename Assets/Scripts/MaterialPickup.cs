using UnityEngine;

public class MaterialPickup : MonoBehaviour
{
    public int value = 5;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.AddMaterial(value);
        UIManager.Instance.UpdateHUD();
        Destroy(gameObject);
    }
}
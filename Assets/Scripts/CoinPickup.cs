using UnityEngine;

/// <summary>
/// Pickup de moneda. Suma al contador del GameManager al ser tocada.
/// Tiene animacion de rotacion y bobbing igual que la tinta.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("Animacion Visual")]
    public float rotateSpeed = 120f;
    public float bobHeight   = 0.2f;
    public float bobSpeed    = 2.5f;

    private Vector3 _startPos;

    // -----------------------------------------------------------------------
    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCoin();
            SoundManager.Instance.PlaySound("CoinCollect");
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

/// <summary>
/// Pickup de tinta. Agrega tinta al jugador al ser tocado.
/// Tiene animacion de rotacion y bobbing vertical.
/// </summary>
public class InkPickup : MonoBehaviour
{
    [Header("Valor")]
    public float inkAmount = 20f;  // Cuanta tinta da al recolectarla

    [Header("Animacion Visual")]
    public float rotateSpeed = 90f;   // Grados por segundo de rotacion
    public float bobHeight   = 0.3f;  // Altura del movimiento vertical
    public float bobSpeed    = 2f;    // Velocidad del bobbing

    private Vector3 _startPos;

    // -----------------------------------------------------------------------
    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Rotar sobre el eje Y
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // Subir y bajar suavemente (seno)
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats.Instance.AddInk(inkAmount);
            Destroy(gameObject);
        }
    }
}

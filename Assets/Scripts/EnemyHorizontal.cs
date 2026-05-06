using UnityEngine;

public class EnemyHorizontal : MonoBehaviour
{
    public float speed = 3f;
    public float distance = 4f;

    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * direction * Time.deltaTime);

        float traveled = transform.position.x - startPos.x;
        if (traveled >= distance || traveled <= -distance)
            direction *= -1;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerDeath>().Die();
    }
}
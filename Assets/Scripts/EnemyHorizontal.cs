using UnityEngine;

public class EnemyHorizontal : MonoBehaviour
{
    public float speed = 3f;
    public float distance = 4f;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 target;

    void Start()
    {
        pointA = transform.position + Vector3.right * distance;
        pointB = transform.position - Vector3.right * distance;
        target = pointA;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
            target = target == pointA ? pointB : pointA;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.GetComponent<PlayerDeath>().Die();
    }
}
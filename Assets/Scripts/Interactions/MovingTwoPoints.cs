using UnityEngine;

public class SawTrapMover : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private Transform target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        // Движемся к цели
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Если практически достигли цели — ставим позицию ровно и меняем target
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            transform.position = target.position;
            target = target == pointA ? pointB : pointA;
        }
    }

}

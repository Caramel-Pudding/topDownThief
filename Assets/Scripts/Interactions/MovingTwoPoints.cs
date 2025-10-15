using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TwoPointsMovement : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private AudioClip movingSound;

    private Transform target;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        target = pointB;
        audioSource.clip = movingSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        // Движемся к цели
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Если практически достигли цели — ставим позицию ровно и меняем target
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            transform.position = target.position;
            target = target == pointA ? pointB : pointA;
        }
    }

}

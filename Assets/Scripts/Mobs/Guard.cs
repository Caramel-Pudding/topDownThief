using UnityEngine;

public class GuardController : MonoBehaviour
{
    public GuardConfig config;
    public Transform visionCone; // ссылка на дочерний объект с коллайдером
    public LayerMask playerMask;
    public LayerMask obstaclesMask;

    enum State { Idle, Detecting, Chasing }
    State state = State.Idle;
    float detectionProgress = 0f;
    Transform player;

    void Awake() => player = GameObject.FindGameObjectWithTag("Player").transform;

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                if (PlayerInCone()) { state = State.Detecting; }
                break;
            case State.Detecting:
                if (PlayerInCone())
                    detectionProgress += Time.deltaTime / config.detectionTime;
                else
                    detectionProgress -= Time.deltaTime / config.loseTime;

                detectionProgress = Mathf.Clamp01(detectionProgress);

                if (detectionProgress >= 1f)
                    state = State.Chasing;
                else if (detectionProgress <= 0f)
                    state = State.Idle;
                break;
            case State.Chasing:
                ChasePlayer();
                // Можно добавить потерю игрока по LOS
                break;
        }
    }

    bool PlayerInCone()
    {
        // Простейшая реализация через OverlapPoint/Collider2D
        Collider2D hit = Physics2D.OverlapPoint(player.position, playerMask);
        if (hit == null) return false;

        // Проверка на преграды (Raycast)
        Vector2 dir = player.position - transform.position;
        var block = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, obstaclesMask);
        return block.collider == null;
    }

    void ChasePlayer()
    {
        Vector2 toPlayer = (player.position - transform.position).normalized;
        transform.position += (Vector3)(toPlayer * config.chaseSpeed * Time.deltaTime);
    }
}

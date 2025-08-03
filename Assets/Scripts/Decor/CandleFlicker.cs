using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CandleFlicker : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;

    [Header("Light")]
    [SerializeField] private Light2D candleLight;
    [SerializeField] private float minMultiplier = 0.7f;
    [SerializeField] private float maxMultiplier = 1.1f;
    [SerializeField] private float flickerRate = 0.12f;

    private SpriteRenderer spriteRenderer;
    private bool useA = true;
    private float timer = 0f;
    private float baseRadius;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseRadius = candleLight.pointLightOuterRadius;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= flickerRate)
        {
            useA = !useA;
            spriteRenderer.sprite = useA ? spriteA : spriteB;
            candleLight.pointLightOuterRadius = baseRadius * Random.Range(minMultiplier, maxMultiplier);
            timer = 0f;
        }
    }
}

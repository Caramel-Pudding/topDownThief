using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LockpickingUI : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private RectTransform needle;
    [SerializeField] private Image successArc;
    [SerializeField] private Canvas canvas;

    [Header("Tuning")]
    [SerializeField] private float speedDegPerSec = 240f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    private InputSystem_Actions controls;
    private float angle;          // 0..360
    private float arcSize;        // degrees
    private float arcCenter;      // degrees
    private int requiredHits;
    private int hits;

    private System.Action<bool> onComplete;
    private Transform follow;

    public void Begin(LockDifficulty difficulty, Transform followTarget, System.Action<bool> onComplete)
    {
        LockDifficultyUtil.GetParams(difficulty, out arcSize, out requiredHits);
        this.onComplete = onComplete;
        this.follow = followTarget;

        if (!canvas) canvas = GetComponent<Canvas>();
        if (canvas) canvas.renderMode = RenderMode.WorldSpace;

        RandomizeArcCenter();
        UpdateArcVisuals();

        controls = new InputSystem_Actions();
        controls.Player.Enable();
        controls.Player.Interact.performed += OnPress;
    }

    void OnDestroy()
    {
        if (controls != null)
        {
            controls.Player.Interact.performed -= OnPress;
            controls.Player.Disable();
            controls.Dispose();
        }
    }

    void Update()
    {
        angle += speedDegPerSec * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        if (needle) needle.localEulerAngles = new Vector3(0f, 0f, -angle);
        if (follow) transform.position = follow.position + worldOffset;
    }

    private void OnPress(InputAction.CallbackContext _)
    {
        float delta = Mathf.DeltaAngle(angle, arcCenter);
        bool inside = Mathf.Abs(delta) <= arcSize * 0.5f;

        if (inside)
        {
            hits++;
            if (hits >= requiredHits) Complete(true);
            else RandomizeArcCenter();
        }
        else
        {
            Complete(false);
        }
    }

    private void Complete(bool success)
    {
        onComplete?.Invoke(success);
        Destroy(gameObject);
    }

    private void RandomizeArcCenter()
    {
        arcCenter = Random.Range(0f, 360f);
        UpdateArcVisuals();
    }

private void UpdateArcVisuals()
{
    if (!successArc) return;

    successArc.type = Image.Type.Filled;
    successArc.fillMethod = Image.FillMethod.Radial360;
    successArc.fillOrigin = (int)Image.Origin360.Top; // 0° вверх
    successArc.fillClockwise = true;

    successArc.fillAmount = Mathf.Clamp01(arcSize / 360f);

    // ВАЖНО: рисуем сектор, центрированный на arcCenter
    float start = arcCenter - arcSize * 0.5f;               // начало сектора
    successArc.rectTransform.localEulerAngles = new Vector3(0f, 0f, -start);
}

}

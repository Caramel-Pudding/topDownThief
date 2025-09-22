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

    [Header("Audio")]
    [SerializeField] private AudioClip startClip;
    [SerializeField] private AudioClip hitClip;      // partial progress (inside but not completed)
    [SerializeField] private AudioClip successClip;  // completed lock
    [SerializeField] private AudioClip failClip;     // failed lock
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource audioSource;

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

        EnsureAudioSource();

        // Play start SFX
        if (startClip) audioSource.PlayOneShot(startClip, volume);

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
            // Feedback for progress (not final yet)
            if (hits < requiredHits && hitClip)
                audioSource.PlayOneShot(hitClip, volume);

            if (hits >= requiredHits)
                Complete(true);
            else
                RandomizeArcCenter();
        }
        else
        {
            Complete(false);
        }
    }

    private void Complete(bool success)
    {
        // Fire callback first (game logic), then handle audio/cleanup.
        onComplete?.Invoke(success);

        float delay = 0.05f;
        if (success && successClip)
        {
            audioSource.PlayOneShot(successClip, volume);
            delay = Mathf.Max(delay, successClip.length);
        }
        else if (!success && failClip)
        {
            audioSource.PlayOneShot(failClip, volume);
            delay = Mathf.Max(delay, failClip.length);
        }

        // Stop input and hide visuals immediately
        if (controls != null)
        {
            controls.Player.Interact.performed -= OnPress;
            controls.Player.Disable();
        }

        if (canvas) canvas.enabled = false;

        // Destroy after the clip finishes so the sound is not cut off
        Destroy(gameObject, delay);
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
        successArc.fillOrigin = (int)Image.Origin360.Top;
        successArc.fillClockwise = true;

        successArc.fillAmount = Mathf.Clamp01(arcSize / 360f);

        float start = arcCenter - arcSize * 0.5f;
        successArc.rectTransform.localEulerAngles = new Vector3(0f, 0f, -start);
    }

    private void EnsureAudioSource()
    {
        if (!audioSource)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for consistent volume
        }
    }
}

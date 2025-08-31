using UnityEngine;
using UnityEngine.UI;

public class DetectionIndicatorUI : MonoBehaviour
{
    [Header("Refs")]
    public GuardDetector detector;

    [Header("UI Elements")]
    public Image bg;
    public Image progress;
    public Image detectedIcon;

    void Awake()
    {
        if (!detector) detector = GetComponentInParent<GuardDetector>();

        // start hidden
        bg.enabled = false;
        progress.enabled = false;
        detectedIcon.enabled = false;
    }

    void Update()
    {
        if (!detector) return;

        if (detector.IsSpotted)
        {
            // fully spotted
            bg.enabled = true;
            progress.enabled = false;
            detectedIcon.enabled = true;
        }
        else if (detector.Progress > 0f)
        {
            // partially spotted
            bg.enabled = true;
            progress.enabled = true;
            detectedIcon.enabled = false;

            progress.fillAmount = detector.Progress;
        }
        else
        {
            // not spotted at all
            bg.enabled = false;
            progress.enabled = false;
            detectedIcon.enabled = false;
        }
    }
}

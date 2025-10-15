using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Win : MonoBehaviour
{
    private InputSystem_Actions controls;

    [Header("UI")]
    public TMP_Text promptText;
    public TMP_Text winMessage;
    public Slider progressBar;

    [Header("Win Settings")]
    public float holdDuration = 3f;
    public string winMessageFormat = "Victory!\nGold collected: {0}";
    [Tooltip("Inventory item id used as gold/currency.")]
    public string goldItemId = "gold";

    [Header("Interaction")]
    public string playerTag = "Player";

    private bool playerInside;
    private bool isHolding;
    private float holdTimer;
    private bool winTriggered;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += _ => isHolding = true;
        controls.Player.Interact.canceled += _ =>
        {
            isHolding = false;
            holdTimer = 0f; // Reset timer on release
        };

        if (promptText) promptText.enabled = false;

        if (progressBar == null)
        {
            progressBar = GetComponentInChildren<Slider>();
        }

        if (progressBar) progressBar.gameObject.SetActive(false);
        if (winMessage) winMessage.enabled = false;
    }

    void OnEnable()
    {
        try { controls.Player.Enable(); } catch {}
    }

    void OnDisable()
    {
        try { controls.Player.Disable(); } catch {}
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = true;
            UpdatePrompt(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        isHolding = false;
        holdTimer = 0f;

        UpdatePrompt(false);
        if (progressBar) { progressBar.value = 0f; progressBar.gameObject.SetActive(false); }
    }

    void Update()
    {
        if (!playerInside || winTriggered) return;

        if (isHolding)
        {
            holdTimer += Time.unscaledDeltaTime;
            if (progressBar)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.value = Mathf.Clamp01(holdTimer / holdDuration);
            }

            if (holdTimer >= holdDuration)
                TriggerWin();
        }
        else
        {
            // When not holding, ensure the progress bar is hidden.
            if (progressBar && progressBar.gameObject.activeSelf)
            {
                progressBar.value = 0f;
                progressBar.gameObject.SetActive(false);
            }
        }
    }

    void TriggerWin()
    {
        winTriggered = true;

        if (promptText) promptText.enabled = false;
        if (progressBar) progressBar.gameObject.SetActive(false);

        int gold = GetGoldAmount();
        if (winMessage)
        {
            winMessage.text = string.Format(winMessageFormat, gold);
            winMessage.enabled = true;
        }

        Time.timeScale = 0f;
    }

    private int GetGoldAmount()
    {
        if (InventoryManager.Instance == null || string.IsNullOrEmpty(goldItemId))
            return 0;

        return InventoryManager.Instance.GetItemCount(goldItemId);
    }

    private void UpdatePrompt(bool show)
    {
        if (!promptText) return;

        if (show)
        {
            string keyName = "E"; // Default
            try
            {
                keyName = controls.Player.Interact.GetBindingDisplayString(0);
                // Sanitize the binding string, e.g., "Press E" -> "E"
                if (keyName.StartsWith("Press "))
                {
                    keyName = keyName.Substring("Press ".Length);
                }
            }
            catch
            {
                // Keep default
            }
            promptText.text = $"Hold {keyName} to win...";
        }
        promptText.enabled = show;
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class WinTileInteraction : MonoBehaviour
{
    private InputSystem_Actions controls;
    public TMP_Text promptText;
    public TMP_Text winMessage;
    public Slider progressBar;
    public float holdDuration = 3f;
    public string playerTag = "Player";

    private bool playerInside;
    private bool isHolding;
    private float holdTimer;
    private bool winTriggered;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += _ => isHolding = true;
        controls.Player.Interact.canceled += _ => isHolding = false;
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            holdTimer = 0f;
            promptText.enabled = false;
            progressBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerInside || winTriggered) return;

        var keyName = controls.Player.Interact.bindings[0].ToDisplayString();
        promptText.text = $"Hold {keyName} to win...";
        promptText.enabled = true;

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            progressBar.gameObject.SetActive(true);
            progressBar.value = Mathf.Clamp01(holdTimer / holdDuration);

            if (holdTimer >= holdDuration)
                TriggerWin();
        }
        else
        {
            holdTimer = 0f;
            progressBar.value = 0f;
        }
    }

    void TriggerWin()
    {
        winTriggered = true;
        promptText.enabled = false;
        progressBar.gameObject.SetActive(false);
        winMessage.text = "Victory!";
        winMessage.enabled = true;
        Time.timeScale = 0f;
    }
}
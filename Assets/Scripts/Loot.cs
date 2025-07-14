using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Loot : MonoBehaviour
{
    private InputSystem_Actions controls;
    public Collider2D winTileTrigger;
    public TMP_Text promptText;
    public TMP_Text winMessage;
    public float holdDuration = 3f;
    public string playerTag = "Player";

    private bool playerInside = false;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private bool winTriggered = false;

    void Awake()
    {
        winMessage.enabled = false;
        Debug.Log("1Your message here");
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += _ => isHolding = true;
        controls.Player.Interact.canceled += _ => isHolding = false;
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = true;
            promptText.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            holdTimer = 0f;
            promptText.enabled = false;
        }
    }

    void Update()
    {
        if (!playerInside || winTriggered) return;

        var bindingDisplay = controls.Player.Interact.bindings[0].ToDisplayString();
        promptText.text = $"Hold {bindingDisplay} to win...";

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
                TriggerWin();
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void TriggerWin()
    {
        winTriggered = true;
        promptText.enabled = false;
        winMessage.text = "Victory!";
        winMessage.enabled = true;
        Time.timeScale = 0f;
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Loot : MonoBehaviour
{
    private InputSystem_Actions controls;

    [Header("UI")]
    public TMP_Text promptText;

    [Header("Interaction")]
    public string playerTag = "Player";

    [Header("Item Settings")]
    public string itemId = "key";

    private bool playerInside;
    private bool interactionComplete;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += _ =>
        {
            if (playerInside && !interactionComplete)
                CompleteInteraction();
        };
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void OnTriggerEnter2D(Collider2D body)
    {
        if (body.CompareTag(playerTag))
        {
            playerInside = true;
            UpdatePrompt(true);
        }
    }

    void OnTriggerExit2D(Collider2D body)
    {
        if (body.CompareTag(playerTag))
        {
            playerInside = false;
            UpdatePrompt(false);
        }
    }

    private void CompleteInteraction()
    {
        interactionComplete = true;
        UpdatePrompt(false);

        InventoryManager.Instance.AddItem(itemId);
        Destroy(gameObject);
    }

    private void UpdatePrompt(bool show)
    {
        if (!promptText) return;
        if (!show)
        {
            promptText.enabled = false;
            return;
        }

        var keyName = controls.Player.Interact.bindings.Count > 0
            ? controls.Player.Interact.bindings[0].ToDisplayString()
            : "Interact";
        promptText.text = keyName;
        promptText.enabled = true;
    }
}

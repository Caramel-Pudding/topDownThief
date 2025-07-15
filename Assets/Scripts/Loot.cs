using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Loot : MonoBehaviour
{
    private InputSystem_Actions controls;
    public TMP_Text promptText;
    public Slider progressBar;
    public float holdDuration = 3f;
    public string playerTag = "Player";

    [Header("Item Settings")]   
    public string itemId = "key";

    private bool playerInside;
    private bool isHolding;
    private float holdTimer;
    private bool interactionComplete;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += _ => isHolding = true;
        controls.Player.Interact.canceled += _ => DropInteraction();
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void OnTriggerEnter2D(Collider2D body)
    {
        if (body.CompareTag(playerTag))
            playerInside = true;
    }

    void OnTriggerExit2D(Collider2D body)
    {
        if (body.CompareTag(playerTag))
        {
            playerInside = false;
            ResetInteractionUI();
        }
    }

    void Update()
    {
        if (!playerInside || interactionComplete) return;

        var keyName = controls.Player.Interact.bindings[0].ToDisplayString();
        promptText.text = $"Hold {keyName}...";
        promptText.enabled = true;

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            ShowProgress(true);
            progressBar.value = Mathf.Clamp01(holdTimer / holdDuration);

            if (holdTimer >= holdDuration)
                CompleteInteraction();
        }
        else
        {
            holdTimer = 0f;
            progressBar.value = 0f;
        }
    }

    private void CompleteInteraction()
    {
        interactionComplete = true;
        promptText.enabled = false;
        ShowProgress(false);

        InventoryManager.Instance.AddItem(itemId);
        Destroy(gameObject);
    }

    private void ShowProgress(bool show)
    {
        progressBar.gameObject.SetActive(show);
    }

    private void DropInteraction()
    {
        isHolding = false;
        ShowProgress(false);
    }

    private void ResetInteractionUI()
    {
        holdTimer = 0f;
        promptText.enabled = false;
        progressBar.gameObject.SetActive(false);
    }
}
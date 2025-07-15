using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))] // this is your trigger collider
public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private string keyId = "key";      // inventory key ID
    [SerializeField] private string playerTag = "Player";   // player tag
    [SerializeField] private TMP_Text promptText;                // UI prompt
    [SerializeField] private Sprite openDoorSprite;            // sprite after opening

    private InputSystem_Actions controls;
    private bool playerInside;
    private bool doorOpened;
    private Collider2D solidCollider;   // the “real” door collider (isTrigger = false)
    private SpriteRenderer doorSprite;      // the door’s SpriteRenderer

    void Awake()
    {
        // cache input
        controls = new InputSystem_Actions();
        controls.Player.Interact.performed += _ => TryOpenDoor();

        // find the solid (non-trigger) collider on this same GameObject
        foreach (var col in GetComponents<Collider2D>())
        {
            if (!col.isTrigger)
            {
                solidCollider = col;
                break;
            }
        }

        // grab the sprite renderer (on this object or in children)
        doorSprite = GetComponent<SpriteRenderer>()
                  ?? GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || doorOpened) return;
        playerInside = true;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        promptText.enabled = false;
    }

    void Update()
    {
        if (playerInside && !doorOpened)
            ShowPrompt();
    }

    private void ShowPrompt()
    {
        if (InventoryManager.Instance.HasItem(keyId))
        {
            var keyName = controls.Player.Interact.bindings[0].ToDisplayString();
            promptText.text = $"Press {keyName} to open door";
        }
        else
        {
            promptText.text = "Door requires a key";
        }
        promptText.enabled = true;
    }

    private void TryOpenDoor()
    {
        if (!playerInside || doorOpened) return;

        if (InventoryManager.Instance.HasItem(keyId))
        {
            InventoryManager.Instance.RemoveItem(keyId);
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        doorOpened = true;
        promptText.enabled = false;

        // swap to the open sprite
        if (openDoorSprite != null && doorSprite != null)
            doorSprite.sprite = openDoorSprite;

        // disable the solid collider so the player can pass
        if (solidCollider != null)
            solidCollider.enabled = false;

        // optionally disable the trigger too, if you don't need it anymore
        GetComponent<Collider2D>().enabled = false;
    }
}

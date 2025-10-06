
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DoorPromptController : MonoBehaviour
{
    [Header("UI")][SerializeField] private TMP_Text promptText;

    private InputSystem_Actions controls;
    private string cachedBinding = "E";

    void Awake()
    {
        controls = new InputSystem_Actions();
        CacheKeyBinding();
        Clear();
    }

    void Start()
    {
        Clear(); // Ensure it's clear on first frame
    }

    public void UpdatePrompt(DoorController door, DoorLockingSystem lockingSystem, bool canClose)
    {
        if (!promptText) return;

        string text = GetPromptText(door, lockingSystem, canClose);

        if (string.IsNullOrEmpty(text))
        {
            Clear();
        }
        else
        {
            promptText.text = text;
            promptText.enabled = true;
        }
    }

    public void Clear()
    {
        if (!promptText) return;
        promptText.text = string.Empty;
        promptText.enabled = false;
    }

    private string GetPromptText(DoorController door, DoorLockingSystem lockingSystem, bool canClose)
    {
        if (door.IsOpen)
        {
            return canClose ? $"Press {cachedBinding} to close" : string.Empty;
        }

        if (lockingSystem.IsEffectivelyUnlocked)
        {
            return $"Press {cachedBinding} to open";
        }

        switch (lockingSystem.Mode)
        {
            case DoorLockingSystem.DoorAccessMode.Pickable:
                return $"Press {cachedBinding} to pick lock";

            case DoorLockingSystem.DoorAccessMode.KeyRequired:
                string keyLabel = string.IsNullOrEmpty(lockingSystem.KeyDisplayName) ? lockingSystem.RequiredKeyId : lockingSystem.KeyDisplayName;
                if (lockingSystem.HasKey())
                {
                    return lockingSystem.ConsumeKey
                        ? $"Press {cachedBinding} to use {keyLabel} and open"
                        : $"Press {cachedBinding} to open (has {keyLabel})";
                }
                else if (lockingSystem.AllowPickIfNoKey && lockingSystem.LockComponent != null)
                {
                    return $"Press {cachedBinding} to pick lock (missing {keyLabel})";
                }
                else
                {
                    return $"{keyLabel} required";
                }
        }

        return string.Empty;
    }

    private void CacheKeyBinding()
    {
        try
        {
            var options = InputBinding.DisplayStringOptions.DontIncludeInteractions;
            var mask = InputBinding.MaskByGroup("Keyboard&Mouse");
            var binding = controls.Player.Interact.GetBindingDisplayString(options: options, bindingMask: mask);

            if (!string.IsNullOrWhiteSpace(binding)) cachedBinding = binding.Trim();
        }
        catch
        {
            cachedBinding = "E"; // Fallback
        }
    }

    public string GetPrompt(object actor)
    {
        return (promptText && promptText.enabled) ? promptText.text : string.Empty;
    }
}

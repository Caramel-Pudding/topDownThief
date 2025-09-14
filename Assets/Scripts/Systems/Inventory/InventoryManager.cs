using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<string, int> items = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // === Add ===
    public bool AddItem(string itemId)
    {
        return AddItem(itemId, 1);
    }

    public bool AddItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;

        if (items.ContainsKey(itemId))
            items[itemId] += amount;
        else
            items[itemId] = amount;

        Debug.Log($"Added {amount} x {itemId}. Total: {items[itemId]}");
        return true;
    }

    // === Has ===
    public bool HasItem(string itemId)
    {
        return items.ContainsKey(itemId) && items[itemId] > 0;
    }

    public int GetItemCount(string itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : 0;
    }

    // === Remove ===
    public bool RemoveItem(string itemId)
    {
        return RemoveItem(itemId, 1);
    }

    public bool RemoveItem(string itemId, int amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;
        if (!items.ContainsKey(itemId)) return false;

        items[itemId] -= amount;
        if (items[itemId] <= 0)
            items.Remove(itemId);

        Debug.Log($"Removed {amount} x {itemId}. Left: {GetItemCount(itemId)}");
        return true;
    }

    // === Debug ===
    public void PrintInventory()
    {
        Debug.Log("=== Inventory Contents ===");
        if (items.Count == 0)
        {
            Debug.Log("Empty");
            return;
        }

        foreach (var kv in items)
            Debug.Log($"{kv.Key} x{kv.Value}");
    }
}

using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<string, int> items = new Dictionary<string, int>();

    void Awake()
    {
        // Ensure this is a singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(string itemId)
    {
        if (items.ContainsKey(itemId))
            items[itemId]++;
        else
            items[itemId] = 1;

        Debug.Log($"Added {itemId}. Total: {items[itemId]}");
    }

    public bool HasItem(string itemId)
    {
        return items.ContainsKey(itemId) && items[itemId] > 0;
    }

    public void RemoveItem(string itemId)
    {
        if (!items.ContainsKey(itemId)) return;
        items[itemId]--;
        if (items[itemId] <= 0) items.Remove(itemId);
    }

    public void PrintInventory()
    {
        Debug.Log("=== Inventory Contents ===");
        foreach (var kv in items)
            Debug.Log($"{kv.Key} x{kv.Value}");
    }
}

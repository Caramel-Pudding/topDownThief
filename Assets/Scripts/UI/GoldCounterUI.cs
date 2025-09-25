using TMPro;
using UnityEngine;

public class GoldCounterUI : MonoBehaviour
{
    [SerializeField] private string goldItemId = "gold";
    [SerializeField] private TextMeshProUGUI text;

    void Update()
    {
        if (InventoryManager.Instance == null) return;

        int gold = InventoryManager.Instance.GetItemCount(goldItemId);
        text.text = $"Gold: {gold}";
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Game/Chest Loot Config", fileName = "ChestLoot")]
public class ChestLootConfig : ScriptableObject
{
    public ItemStack[] items;
}
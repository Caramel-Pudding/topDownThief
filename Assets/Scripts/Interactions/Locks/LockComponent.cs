using UnityEngine;

public class LockComponent : MonoBehaviour
{
    [SerializeField] private bool locked = true;
    [SerializeField] private LockDifficulty difficulty = LockDifficulty.Medium;

    public bool IsLocked => locked;
    public LockDifficulty Difficulty => difficulty;

    public void ForceUnlock() => locked = false;
    public void ForceLock() => locked = true;
}

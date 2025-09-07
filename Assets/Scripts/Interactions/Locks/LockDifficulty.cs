using UnityEngine;

public enum LockDifficulty { Easy, Medium, Hard }

public static class LockDifficultyUtil
{
    public static void GetParams(LockDifficulty d, out float arcDegrees, out int requiredHits)
    {
        switch (d)
        {
            case LockDifficulty.Easy:   arcDegrees = 90f; requiredHits = 1; break;
            case LockDifficulty.Medium: arcDegrees = 60f; requiredHits = 2; break;
            default:                    arcDegrees = 35f; requiredHits = 3; break;
        }
    }
}

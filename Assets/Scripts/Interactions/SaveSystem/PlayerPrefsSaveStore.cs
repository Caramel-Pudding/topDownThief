using UnityEngine;

public class PlayerPrefsSaveStore : ISaveStore
{
    public bool TryGetBool(string key, out bool value)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            value = default;
            return false;
        }

        value = PlayerPrefs.GetInt(key, 0) != 0;
        return true;
    }

    public void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}

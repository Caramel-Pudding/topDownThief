public interface ISaveStore
{
    bool TryGetBool(string key, out bool value);
    void SetBool(string key, bool value);
}

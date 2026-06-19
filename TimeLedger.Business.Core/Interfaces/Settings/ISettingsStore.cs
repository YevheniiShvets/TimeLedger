namespace TimeLedger.Core.Interfaces.Settings;

public interface ISettingsStore
{
    public T? Get<T>(string key);
    public void Set<T>(string key, T value);
}
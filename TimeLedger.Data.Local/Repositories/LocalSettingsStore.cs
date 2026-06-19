using System.Text.Json;
using Microsoft.Data.Sqlite;
using TimeLedger.Core.Interfaces.Settings;

namespace TimeLedger.Data.Local.Repositories;

public class LocalSettingsStore(string filePath) : ISettingsStore
{
    private string ConnectionString => $"Data Source={filePath}";
 
    public T? Get<T>(string key)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
 
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Value FROM Settings WHERE Key == @key";
        cmd.Parameters.AddWithValue("@key", key);
 
        var result = cmd.ExecuteScalar();
        return result is null ? default : JsonSerializer.Deserialize<T>((string)result);
    }
 
    public void Set<T>(string key, T value)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
 
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
                          INSERT INTO Settings (Key, Value)
                          VALUES (@key, @value)
                          ON CONFLICT(Key) DO UPDATE SET Value = @value
                          """;
        cmd.Parameters.AddWithValue("@key", key);
        cmd.Parameters.AddWithValue("@value", JsonSerializer.Serialize(value));
        cmd.ExecuteNonQuery();
    }

}
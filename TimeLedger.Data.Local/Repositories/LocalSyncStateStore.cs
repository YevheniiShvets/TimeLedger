using TimeLedger.Core.Interfaces.Synchronisation;
using Microsoft.Data.Sqlite;

namespace TimeLedger.Data.Local.Repositories;

public class LocalSyncStateStore(string filePath) : ISyncStateStore
{
    private string ConnectionString => $"Data Source={filePath}";
 
    public DateTime? GetLastSyncedAt()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
 
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM SyncState;";
 
        var result = cmd.ExecuteScalar();
        return result is null ? null : DateTime.Parse((string)result);
    }
 
    public void SetLastSyncedAt(DateTime at)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
                          INSERT INTO SyncState
                          VALUES (@at)
                          ON CONFLICT DO UPDATE SET LastSyncedAt = @at
                          """;
        cmd.Parameters.AddWithValue("@at", at.ToString("O"));
        cmd.ExecuteNonQuery();
    }

}
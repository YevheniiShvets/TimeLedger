using Microsoft.Data.Sqlite;

namespace TimeLedger.Data.Local.Repositories;

public class LocalDatabaseInitializer(string filePath)
{
    private string ConnectionString => $"Data Source={filePath}";

    public void Initialize()
    {
        // Creating a connection to a non-existent file creates it automatically
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = Schema;
        cmd.ExecuteNonQuery();
    }

    private const string Schema = """
        -- Events owned by the local user or a group
        CREATE TABLE IF NOT EXISTS Events (
            Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
            Title                   TEXT    NOT NULL,
            Description             TEXT,
            Location                TEXT,
            AllowOverlap            INTEGER NOT NULL DEFAULT 0,     -- bool: 0=false, 1=true
            EventType               INTEGER NOT NULL DEFAULT 1,     -- EventType enum: 1=OneTime, 2=Recurrence, 3=Deadline
            StartTime               TEXT,                           -- ISO 8601, nullable for Deadline
            EndTime                 TEXT,                           -- ISO 8601, nullable for Deadline
            DueAt                   TEXT,                           -- ISO 8601, Deadline only
            RecurrenceFrequency     INTEGER,                        -- RecurrenceFrequency enum, nullable
            RecurrenceInterval      INTEGER,
            RecurrenceValue         TEXT,
            RecurrenceEndTime       TEXT,
            RecurrenceMaxOccurrences INTEGER,
            UpdatedAt               TEXT    NOT NULL,               -- ISO 8601, used for delta sync
            IsDeleted               INTEGER NOT NULL DEFAULT 0      -- soft delete flag for sync
        );


        CREATE INDEX IF NOT EXISTS IX_Events_UpdatedAt
            ON Events (UpdatedAt);


        CREATE TABLE IF NOT EXISTS SyncState (
            LastSyncedAt    TEXT    NOT NULL        -- ISO 8601
        );

        -- General key-value settings store
        CREATE TABLE IF NOT EXISTS Settings (
            Key     TEXT PRIMARY KEY,
            Value   TEXT NOT NULL                   -- JSON-serialized value
        );
        """;
}
using Microsoft.Data.Sqlite;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Data.Local.Repositories;


public class LocalEventRepository(string filePath) : IEventRepository
{
    private string ConnectionString => $"Data Source={filePath}";

    // -------------------------------------------------------------------------
    // GetAll
    // -------------------------------------------------------------------------

    public IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM Events
            WHERE IsDeleted == 0
            ORDER BY StartTime
            """;

        return ReadAll(cmd).ToList();
    }

    // -------------------------------------------------------------------------
    // GetById
    // -------------------------------------------------------------------------

    public Event? GetById(int id, EventOwnerType ownerType, int ownerId)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM Events
            WHERE Id        = @id
              AND IsDeleted = 0
            """;
        cmd.Parameters.AddWithValue("@id",        id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapRow(reader) : null;
    }

    // -------------------------------------------------------------------------
    // Add
    // -------------------------------------------------------------------------

    public Event Add(Event e)
    {
        e.UpdatedAt = DateTime.UtcNow;
        if (e.Id == 0)
            e.Id = GetNextLocalId();

        return Upsert(e);
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public Event Update(Event e)
    {
        e.UpdatedAt = DateTime.UtcNow;

        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Events SET
                Title                    = @title,
                Description              = @description,
                Location                 = @location,
                AllowOverlap             = @allowOverlap,
                EventType                = @eventType,
                StartTime                = @startTime,
                EndTime                  = @endTime,
                DueAt                    = @dueAt,
                RecurrenceFrequency      = @recurrenceFrequency,
                RecurrenceInterval       = @recurrenceInterval,
                RecurrenceValue          = @recurrenceValue,
                RecurrenceEndTime        = @recurrenceEndTime,
                RecurrenceMaxOccurrences = @recurrenceMaxOccurrences,
                UpdatedAt                = @updatedAt,
                IsDeleted                = @isDeleted
            WHERE Id == @id
            """;

        BindWriteParams(cmd, e);
        cmd.Parameters.AddWithValue("@id",        e.Id);
        cmd.Parameters.AddWithValue("@isDeleted", e.IsDeleted ? 1 : 0);
        cmd.ExecuteNonQuery();
        return e;
    }

    // -------------------------------------------------------------------------
    // Delete (hard delete — soft delete is handled by SyncedEventRepository)
    // -------------------------------------------------------------------------

    public void Delete(Event e)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Events WHERE Id == @id";
        cmd.Parameters.AddWithValue("@id", e.Id);
        cmd.ExecuteNonQuery();
    }

    // -------------------------------------------------------------------------
    // Upsert — insert or update by Id, used by SyncService pull phase
    // -------------------------------------------------------------------------

    public Event Upsert(Event e)
    {
        e.UpdatedAt = DateTime.UtcNow;

        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Events (
                Id,
                Title, Description, Location,
                AllowOverlap, EventType,
                StartTime, EndTime, DueAt,
                RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
                RecurrenceEndTime, RecurrenceMaxOccurrences,
                UpdatedAt, IsDeleted
            ) VALUES (
                @id,
                @title, @description, @location,
                @allowOverlap, @eventType,
                @startTime, @endTime, @dueAt,
                @recurrenceFrequency, @recurrenceInterval, @recurrenceValue,
                @recurrenceEndTime, @recurrenceMaxOccurrences,
                @updatedAt, @isDeleted
            )
            ON CONFLICT(Id) DO UPDATE SET
                Title                    = excluded.Title,
                Description              = excluded.Description,
                Location                 = excluded.Location,
                AllowOverlap             = excluded.AllowOverlap,
                EventType                = excluded.EventType,
                StartTime                = excluded.StartTime,
                EndTime                  = excluded.EndTime,
                DueAt                    = excluded.DueAt,
                RecurrenceFrequency      = excluded.RecurrenceFrequency,
                RecurrenceInterval       = excluded.RecurrenceInterval,
                RecurrenceValue          = excluded.RecurrenceValue,
                RecurrenceEndTime        = excluded.RecurrenceEndTime,
                RecurrenceMaxOccurrences = excluded.RecurrenceMaxOccurrences,
                UpdatedAt                = excluded.UpdatedAt,
                IsDeleted                = excluded.IsDeleted
            """;

        cmd.Parameters.AddWithValue("@id",        e.Id);
        cmd.Parameters.AddWithValue("@isDeleted", e.IsDeleted ? 1 : 0);
        BindWriteParams(cmd, e);
        cmd.ExecuteNonQuery();
        return e;
    }

    // -------------------------------------------------------------------------
    // HasOverlap
    // -------------------------------------------------------------------------

    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludeId,
        EventOwnerType ownerType, int ownerId)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(1) FROM Events
            WHERE IsDeleted   = 0
              AND AllowOverlap = 0
              AND StartTime   < @endTime
              AND EndTime     > @startTime
              AND (@excludeId IS NULL OR Id != @excludeId)
            """;
        
        cmd.Parameters.AddWithValue("@startTime",  startTime.ToString("O"));
        cmd.Parameters.AddWithValue("@endTime",    endTime.ToString("O"));
        cmd.Parameters.AddWithValue("@excludeId",  excludeId.HasValue ? excludeId.Value : DBNull.Value);

        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    // -------------------------------------------------------------------------
    // GetByType
    // -------------------------------------------------------------------------

    public IEnumerable<Event> GetByType(EventType type, EventOwnerType ownerType, int ownerId)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM Events
            WHERE EventType = @eventType
              AND IsDeleted = 0
            """;
        cmd.Parameters.AddWithValue("@eventType", (int)type);

        return ReadAll(cmd).ToList();
    }

    // -------------------------------------------------------------------------
    // GetUpdatedSince — returns active AND soft-deleted for sync push/pull
    // -------------------------------------------------------------------------

    public IEnumerable<Event> GetUpdatedSince(DateTime since, EventOwnerType ownerType, int ownerId)
    {
        using var connection = Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM Events
            WHERE UpdatedAt > @since
            """;

        cmd.Parameters.AddWithValue("@since", since.ToString("O"));

        return ReadAll(cmd).ToList();
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }
    
    private static void BindWriteParams(SqliteCommand cmd, Event e)
    {
        cmd.Parameters.AddWithValue("@title",                   e.Title);
        cmd.Parameters.AddWithValue("@description",             e.Description       ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@location",                e.Location          ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@allowOverlap",            e.AllowOverlap ? 1 : 0);
        cmd.Parameters.AddWithValue("@eventType",               (int)e.EventType);
        cmd.Parameters.AddWithValue("@startTime",               e.StartTime?.ToString("O")         ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@endTime",                 e.EndTime?.ToString("O")           ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@dueAt",                   e.DueAt?.ToString("O")             ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@recurrenceFrequency",     e.RecurrenceFrequency.HasValue
                                                                    ? (int)e.RecurrenceFrequency.Value
                                                                    : DBNull.Value);
        cmd.Parameters.AddWithValue("@recurrenceInterval",      e.RecurrenceInterval               ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@recurrenceValue",         e.RecurrenceValue                  ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@recurrenceEndTime",       e.RecurrenceEndTime?.ToString("O") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@recurrenceMaxOccurrences",e.RecurrenceMaxOccurrences         ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@updatedAt",               e.UpdatedAt.ToString("O"));
    }


    private static IEnumerable<Event> ReadAll(SqliteCommand cmd)
    {
        using var reader = cmd.ExecuteReader();
        var results = new List<Event>();
        while (reader.Read())
            results.Add(MapRow(reader));
        return results;
    }

    private int GetNextLocalId()
    {
        using var connection = Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT MIN(Id) FROM Events WHERE Id < 0;";
        var result = command.ExecuteScalar();

        var lowestExisting = result == null || result is DBNull ? 0 : Convert.ToInt32(result);
        return Math.Min(lowestExisting, 0) - 1;
    }
    
    private static Event MapRow(SqliteDataReader r) => new()
    {
        Id                       = r.GetInt32(r.GetOrdinal("Id")),
        Title                    = r.GetString(r.GetOrdinal("Title")),
        Description              = r.IsDBNull(r.GetOrdinal("Description"))  ? null : r.GetString(r.GetOrdinal("Description")),
        Location                 = r.IsDBNull(r.GetOrdinal("Location"))     ? null : r.GetString(r.GetOrdinal("Location")),
        AllowOverlap             = r.GetInt32(r.GetOrdinal("AllowOverlap")) == 1,
        EventType                = (EventType)r.GetInt32(r.GetOrdinal("EventType")),
        StartTime                = r.IsDBNull(r.GetOrdinal("StartTime"))    ? null : DateTime.Parse(r.GetString(r.GetOrdinal("StartTime"))),
        EndTime                  = r.IsDBNull(r.GetOrdinal("EndTime"))      ? null : DateTime.Parse(r.GetString(r.GetOrdinal("EndTime"))),
        DueAt                    = r.IsDBNull(r.GetOrdinal("DueAt"))        ? null : DateTime.Parse(r.GetString(r.GetOrdinal("DueAt"))),
        RecurrenceFrequency      = r.IsDBNull(r.GetOrdinal("RecurrenceFrequency"))
                                       ? null
                                       : (RecurrenceFrequency)r.GetInt32(r.GetOrdinal("RecurrenceFrequency")),
        RecurrenceInterval       = r.IsDBNull(r.GetOrdinal("RecurrenceInterval"))      ? null : r.GetInt32(r.GetOrdinal("RecurrenceInterval")),
        RecurrenceValue          = r.IsDBNull(r.GetOrdinal("RecurrenceValue"))          ? null : r.GetString(r.GetOrdinal("RecurrenceValue")),
        RecurrenceEndTime        = r.IsDBNull(r.GetOrdinal("RecurrenceEndTime"))
                                       ? null
                                       : DateTime.Parse(r.GetString(r.GetOrdinal("RecurrenceEndTime"))),
        RecurrenceMaxOccurrences = r.IsDBNull(r.GetOrdinal("RecurrenceMaxOccurrences")) ? null : r.GetInt32(r.GetOrdinal("RecurrenceMaxOccurrences")),
        UpdatedAt                = DateTime.Parse(r.GetString(r.GetOrdinal("UpdatedAt"))),
        IsDeleted                = r.GetInt32(r.GetOrdinal("IsDeleted")) == 1,
    };
}
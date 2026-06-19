using System.Data;
using BusinessCollaboration.Interfaces.Event;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TimeLedger.Core.Models.Event;

namespace TimeLedger.Infrastructure.Repositories;

public class EventRepository(IConfiguration configuration) : IRemoteEventRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Database is not working");

    public IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences, IsDeleted, UpdatedAt
        FROM Events
        WHERE OwnerType = @OwnerType AND OwnerId = @OwnerId
        ORDER BY StartTime";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;

        using var reader = command.ExecuteReader();
        var events = new List<Event>();
        while (reader.Read())
            events.Add(MapToEvent(reader));
        return events;
    }

    public Event? GetById(int id, EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences, IsDeleted, UpdatedAt
        FROM Events
        WHERE Id = @Id AND OwnerType = @OwnerType AND OwnerId = @OwnerId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return null;
        return MapToEvent(reader);
    }

    public Event Add(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        INSERT INTO Events (OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
                            EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
                            RecurrenceEndTime, RecurrenceMaxOccurrences, IsDeleted, UpdatedAt)
        OUTPUT INSERTED.Id
        VALUES (@OwnerType, @OwnerId, @Title, @Description, @Location, @StartTime, @EndTime, @AllowOverlap,
                @EventType, @DueAt, @RecurrenceFrequency, @RecurrenceInterval, @RecurrenceValue,
                @RecurrenceEndTime, @RecurrenceMaxOccurrences, @IsDeleted, @UpdatedAt)";

        using var command = new SqlCommand(sql, connection);
        BindParams(command, e);
        e.Id = (int)command.ExecuteScalar();
        return e;
    }

    public Event Update(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        UPDATE Events
        SET Title = @Title,
            Description = @Description,
            Location = @Location,
            StartTime = @StartTime,
            EndTime = @EndTime,
            AllowOverlap = @AllowOverlap,
            EventType = @EventType,
            DueAt = @DueAt,
            RecurrenceFrequency = @RecurrenceFrequency,
            RecurrenceInterval = @RecurrenceInterval,
            RecurrenceValue = @RecurrenceValue,
            RecurrenceEndTime = @RecurrenceEndTime,
            RecurrenceMaxOccurrences = @RecurrenceMaxOccurrences,
            IsDeleted = @IsDeleted,
            UpdatedAt = @UpdatedAt
        WHERE Id = @Id AND OwnerType = @OwnerType AND OwnerId = @OwnerId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = e.Id;
        BindParams(command, e);

        var rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
            throw new KeyNotFoundException("Event was not updated because no matching row was found.");

        return e;
    }

    public void Delete(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = "DELETE FROM Events WHERE Id = @Id AND OwnerType = @OwnerType AND OwnerId = @OwnerId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = e.Id;
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)e.OwnerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = e.OwnerId;

        command.ExecuteNonQuery();
    }

    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludedId, EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT COUNT(*)
        FROM Events
        WHERE OwnerType = @OwnerType
          AND OwnerId = @OwnerId
          AND IsDeleted = 0
          AND (@ExcludedId IS NULL OR Id <> @ExcludedId)
          AND StartTime < @EndTime
          AND EndTime > @StartTime";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = endTime;
        command.Parameters.Add("@ExcludedId", SqlDbType.Int).Value = (object?)excludedId ?? DBNull.Value;

        return (int)command.ExecuteScalar() > 0;
    }

    public IEnumerable<int> GetOverlappingOwnerIds(DateTime startTime, DateTime endTime, int? excludedId,
        EventOwnerType ownerType, IEnumerable<int> ownerIds)
    {
        var owners = ownerIds.Distinct().ToList();
        if (owners.Count == 0)
            return [];

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var ownerParameters = owners.Select((_, i) => $"@OwnerId{i}").ToList();
        var inClause = string.Join(", ", ownerParameters);

        var sql = $@"
        SELECT DISTINCT OwnerId
        FROM Events
        WHERE OwnerType = @OwnerType
          AND IsDeleted = 0
          AND OwnerId IN ({inClause})
          AND (@ExcludedId IS NULL OR Id <> @ExcludedId)
          AND StartTime < @EndTime
          AND EndTime > @StartTime";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@ExcludedId", SqlDbType.Int).Value = (object?)excludedId ?? DBNull.Value;
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = endTime;
        for (var i = 0; i < owners.Count; i++)
            command.Parameters.Add(ownerParameters[i], SqlDbType.Int).Value = owners[i];

        using var reader = command.ExecuteReader();
        var result = new List<int>();
        while (reader.Read())
            result.Add(reader.GetInt32(0));
        return result;
    }

    public IEnumerable<Event> GetByType(EventType type, EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences, IsDeleted, UpdatedAt
        FROM Events
        WHERE EventType = @EventType AND OwnerType = @OwnerType AND OwnerId = @OwnerId
        ORDER BY StartTime";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@EventType", SqlDbType.TinyInt).Value = (byte)type;
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;

        using var reader = command.ExecuteReader();
        var events = new List<Event>();
        while (reader.Read())
            events.Add(MapToEvent(reader));
        return events;
    }

    public IEnumerable<Event> GetUpdatedSince(DateTime since, EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences, IsDeleted, UpdatedAt
        FROM Events
        WHERE OwnerType = @OwnerType AND OwnerId = @OwnerId AND UpdatedAt > @Since
        ORDER BY UpdatedAt";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        command.Parameters.Add("@Since", SqlDbType.DateTime2).Value = since;

        using var reader = command.ExecuteReader();
        var events = new List<Event>();
        while (reader.Read())
            events.Add(MapToEvent(reader));
        return events;
    }

    public Event Upsert(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        MERGE Events AS target
        USING (SELECT @Id AS Id) AS source
        ON target.Id = source.Id
        WHEN MATCHED THEN
            UPDATE SET
                Title = @Title, Description = @Description, Location = @Location,
                StartTime = @StartTime, EndTime = @EndTime, AllowOverlap = @AllowOverlap,
                EventType = @EventType, DueAt = @DueAt,
                RecurrenceFrequency = @RecurrenceFrequency, RecurrenceInterval = @RecurrenceInterval,
                RecurrenceValue = @RecurrenceValue, RecurrenceEndTime = @RecurrenceEndTime,
                RecurrenceMaxOccurrences = @RecurrenceMaxOccurrences,
                IsDeleted = @IsDeleted, UpdatedAt = @UpdatedAt
        WHEN NOT MATCHED THEN
            INSERT (OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
                    EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
                    RecurrenceEndTime, RecurrenceMaxOccurrences, IsDeleted, UpdatedAt)
            VALUES (@OwnerType, @OwnerId, @Title, @Description, @Location, @StartTime, @EndTime, @AllowOverlap,
                    @EventType, @DueAt, @RecurrenceFrequency, @RecurrenceInterval, @RecurrenceValue,
                    @RecurrenceEndTime, @RecurrenceMaxOccurrences, @IsDeleted, @UpdatedAt);";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = e.Id;
        BindParams(command, e);
        command.ExecuteNonQuery();
        return e;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static void BindParams(SqlCommand command, Event e)
    {
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)e.OwnerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = e.OwnerId;
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = e.Title;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = (object?)e.Description ?? DBNull.Value;
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 300).Value = (object?)e.Location ?? DBNull.Value;
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = (object?)e.StartTime ?? DBNull.Value;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = (object?)e.EndTime ?? DBNull.Value;
        command.Parameters.Add("@AllowOverlap", SqlDbType.Bit).Value = e.AllowOverlap;
        command.Parameters.Add("@EventType", SqlDbType.TinyInt).Value = (byte)e.EventType;
        command.Parameters.Add("@DueAt", SqlDbType.DateTime2).Value = (object?)e.DueAt ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceFrequency", SqlDbType.TinyInt).Value = (object?)(byte?)e.RecurrenceFrequency ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceInterval", SqlDbType.Int).Value = e.RecurrenceInterval ?? 1;
        command.Parameters.Add("@RecurrenceValue", SqlDbType.NVarChar, -1).Value = (object?)e.RecurrenceValue ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceEndTime", SqlDbType.DateTime2).Value = (object?)e.RecurrenceEndTime ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceMaxOccurrences", SqlDbType.Int).Value = (object?)e.RecurrenceMaxOccurrences ?? DBNull.Value;
        command.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = e.IsDeleted;
        command.Parameters.Add("@UpdatedAt", SqlDbType.DateTime2).Value = e.UpdatedAt;
    }

    private static Event MapToEvent(SqlDataReader reader) => new()
{
    Id = reader.GetInt32(reader.GetOrdinal("Id")),
    OwnerType = (EventOwnerType)reader.GetByte(reader.GetOrdinal("OwnerType")),
    OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
    Title = reader.GetString(reader.GetOrdinal("Title")),
    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
    StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : reader.GetDateTime(reader.GetOrdinal("StartTime")),
    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
    AllowOverlap = reader.GetBoolean(reader.GetOrdinal("AllowOverlap")),
    EventType = (EventType)reader.GetByte(reader.GetOrdinal("EventType")),
    DueAt = reader.IsDBNull(reader.GetOrdinal("DueAt")) ? null : reader.GetDateTime(reader.GetOrdinal("DueAt")),
    RecurrenceFrequency = reader.IsDBNull(reader.GetOrdinal("RecurrenceFrequency")) ? null : (RecurrenceFrequency)reader.GetByte(reader.GetOrdinal("RecurrenceFrequency")),
    RecurrenceInterval = reader.IsDBNull(reader.GetOrdinal("RecurrenceInterval")) ? null : reader.GetInt32(reader.GetOrdinal("RecurrenceInterval")),
    RecurrenceValue = reader.IsDBNull(reader.GetOrdinal("RecurrenceValue")) ? null : reader.GetString(reader.GetOrdinal("RecurrenceValue")),
    RecurrenceEndTime = reader.IsDBNull(reader.GetOrdinal("RecurrenceEndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("RecurrenceEndTime")),
    RecurrenceMaxOccurrences = reader.IsDBNull(reader.GetOrdinal("RecurrenceMaxOccurrences")) ? null : reader.GetInt32(reader.GetOrdinal("RecurrenceMaxOccurrences")),
    IsDeleted = !reader.IsDBNull(reader.GetOrdinal("IsDeleted")) && reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
};
}
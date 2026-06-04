using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TimeLedger.Core.Interfaces.Events;
using TimeLedger.Core.Models.Events;

namespace TimeLedger.Infrastructure.Repositories;

public class EventRepository(IConfiguration configuration) : IEventRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database is not working");

    public IEnumerable<Event> GetAll(EventOwnerType ownerType, int ownerId)
    {
        var events = new List<Event>();
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences
            FROM Events
            WHERE OwnerType = @OwnerType AND OwnerId = @OwnerId
            ORDER BY StartTime";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            events.Add(MapToEvent(reader));
        }
        return events;
    }

    public Event? GetById(int id, EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences
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
        INSERT INTO Events(OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
                          EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
                          RecurrenceEndTime, RecurrenceMaxOccurrences)
        OUTPUT INSERTED.Id
        VALUES (@OwnerType, @OwnerId, @Title, @Description, @Location, @StartTime, @EndTime, @AllowOverlap,
                @EventType, @DueAt, @RecurrenceFrequency, @RecurrenceInterval, @RecurrenceValue,
                @RecurrenceEndTime, @RecurrenceMaxOccurrences)";
        
        using var command = new SqlCommand(sql, connection);
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
        command.Parameters.Add("@RecurrenceValue", SqlDbType.NVarChar).Value = (object?)e.RecurrenceValue ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceEndTime", SqlDbType.DateTime2).Value = (object?)e.RecurrenceEndTime ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceMaxOccurrences", SqlDbType.Int).Value = (object?)e.RecurrenceMaxOccurrences ?? DBNull.Value;
        
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
            RecurrenceMaxOccurrences = @RecurrenceMaxOccurrences
        WHERE Id = @Id AND OwnerType = @OwnerType AND OwnerId = @OwnerId";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = e.Id;
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
        command.Parameters.Add("@RecurrenceValue", SqlDbType.NVarChar).Value = (object?)e.RecurrenceValue ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceEndTime", SqlDbType.DateTime2).Value = (object?)e.RecurrenceEndTime ?? DBNull.Value;
        command.Parameters.Add("@RecurrenceMaxOccurrences", SqlDbType.Int).Value = (object?)e.RecurrenceMaxOccurrences ?? DBNull.Value;
        
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
          AND (@ExcludedId IS NULL OR Id <> @ExcludedId)
          AND StartTime < @EndTime
          AND EndTime > @StartTime";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerType", SqlDbType.TinyInt).Value = (byte)ownerType;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = endTime;
        command.Parameters.Add("@ExcludedId", SqlDbType.Int).Value = (object?)excludedId ?? DBNull.Value;
        
        var count = (int)command.ExecuteScalar();
        return count > 0;
    }

    public IEnumerable<int> GetOverlappingOwnerIds(DateTime startTime, DateTime endTime, int? excludedId, EventOwnerType ownerType, IEnumerable<int> ownerIds)
    {
        var owners = ownerIds.Distinct().ToList();
        if (owners.Count == 0)
            return [];

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var ownerParameters = owners.Select((_, index) => $"@OwnerId{index}").ToList();
        var inClause = string.Join(", ", ownerParameters);
        
        // TODO: What is inClause and how it works?
        
        
        var sql = $@"
        SELECT DISTINCT OwnerId
        FROM Events
        WHERE OwnerType = @OwnerType
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
        {
            command.Parameters.Add(ownerParameters[i], SqlDbType.Int).Value = owners[i];
        }

        using var reader = command.ExecuteReader();
        var result = new List<int>();
        while (reader.Read())
        {
            result.Add(reader.GetInt32(0));
        }
        return result;
    }

    public IEnumerable<Event> GetByType(EventType type, EventOwnerType ownerType, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, OwnerType, OwnerId, Title, Description, Location, StartTime, EndTime, AllowOverlap,
               EventType, DueAt, RecurrenceFrequency, RecurrenceInterval, RecurrenceValue,
               RecurrenceEndTime, RecurrenceMaxOccurrences
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
        {
            events.Add(MapToEvent(reader));
        }
        return events;
    }

    private static Event MapToEvent(SqlDataReader reader) => new ()
        {
            Id = reader.GetInt32(0),
            OwnerType = (EventOwnerType)reader.GetByte(1),
            OwnerId = reader.GetInt32(2),
            Title = reader.GetString(3),
            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
            Location = reader.IsDBNull(5) ? null : reader.GetString(5),
            StartTime = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            EndTime = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
            AllowOverlap = reader.GetBoolean(8),
            EventType = (EventType)reader.GetByte(9),
            DueAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
            RecurrenceFrequency = reader.IsDBNull(11) ? null : (RecurrenceFrequency)reader.GetByte(11),
            RecurrenceInterval = reader.IsDBNull(12) ? null : reader.GetInt32(12),
            RecurrenceValue = reader.IsDBNull(13) ? null : reader.GetString(13),
            RecurrenceEndTime = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
            RecurrenceMaxOccurrences = reader.IsDBNull(15) ? null : reader.GetInt32(15)
        };
}
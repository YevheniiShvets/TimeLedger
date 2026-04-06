using Microsoft.Data.SqlClient;
using TimeLedger.Models;

namespace TimeLedger.Repositories;

public class EventRepository : IEventRepository
{
    private readonly string _connectionString;

    public EventRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IEnumerable<Event> GetAll()
    {
        var events = new List<Event>();
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, Title, Description, Location, StartTime, EndTime, AllowOverlap
            FROM Events
            ORDER BY StartTime";
        
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            events.Add(new Event
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Location = reader.IsDBNull(3) ? null : reader.GetString(3),
                StartTime = reader.GetDateTime(4),
                EndTime = reader.GetDateTime(5),
                AllowOverlap = reader.GetBoolean(6)
            });
        }
        return events;
    }

    public Event? GetById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, Title, Description, Location, StartTime, EndTime, AllowOverlap
        FROM Events
        WHERE Id = @Id";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = command.ExecuteReader();
        
        if (!reader.Read())
            return null;
        return new Event
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            Location = reader.IsDBNull(3) ? null : reader.GetString(3),
            StartTime = reader.GetDateTime(4),
            EndTime = reader.GetDateTime(5),
            AllowOverlap = reader.GetBoolean(6)
        };
    }

    public Event Add(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        INSERT INTO Events(Title, Description, Location, StartTime, EndTime, AllowOverlap)
        OUTPUT INSERTED.Id
        VALUES (@Title, @Description, @Location, @StartTime, @EndTime, @AllowOverlap)";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Title", e.Title);
        command.Parameters.AddWithValue("@Description", (object?)e.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)e.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartTime", e.StartTime);
        command.Parameters.AddWithValue("@EndTime", e.EndTime);
        command.Parameters.AddWithValue("@AllowOverlap", e.AllowOverlap);
        
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
            AllowOverlap = @AllowOverlap
        WHERE Id = @Id";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", e.Id);
        command.Parameters.AddWithValue("@Title", e.Title);
        command.Parameters.AddWithValue("@Description", (object?)e.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)e.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartTime", e.StartTime);
        command.Parameters.AddWithValue("@EndTime", e.EndTime);
        command.Parameters.AddWithValue("@AllowOverlap", e.AllowOverlap);
        
        command.ExecuteNonQuery();
        return e;
    }

    public void Delete(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = "DELETE FROM Events WHERE Id = @Id";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", e.Id);
        
        command.ExecuteNonQuery();
    }

    public bool HasOverlap(DateTime startTime, DateTime endTime, int? excludedId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT COUNT(*) 
        FROM Events
        WHERE (@ExcludedId IS NULL OR Id <> @ExcludedId)
          AND StartTime < @EndTime
          AND EndTime > @StartTime";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@StartTime", startTime);
        command.Parameters.AddWithValue("@EndTime", endTime);
        command.Parameters.AddWithValue("@ExcludedId", (object?)excludedId ?? DBNull.Value);
        
        var count = (int)command.ExecuteScalar();
        return count > 0;
    }
}
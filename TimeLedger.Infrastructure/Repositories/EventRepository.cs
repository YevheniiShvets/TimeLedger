using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Infrastructure.Repositories;

public class EventRepository(IConfiguration configuration) : IEventRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database is not working");

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
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        
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
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = e.Title;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = (object?)e.Description ?? DBNull.Value;
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 300).Value = (object?)e.Location ?? DBNull.Value;
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = e.StartTime;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = e.EndTime;
        command.Parameters.Add("@AllowOverlap", SqlDbType.Bit).Value = e.AllowOverlap;
        
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
        command.Parameters.Add("@Id", SqlDbType.Int).Value = e.Id;
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = e.Title;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = (object?)e.Description ?? DBNull.Value;
        command.Parameters.Add("@Location", SqlDbType.NVarChar, 300).Value = (object?)e.Location ?? DBNull.Value;
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = e.StartTime;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = e.EndTime;
        command.Parameters.Add("@AllowOverlap", SqlDbType.Bit).Value = e.AllowOverlap;
        
        command.ExecuteNonQuery();
        return e;
    }

    public void Delete(Event e)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = "DELETE FROM Events WHERE Id = @Id";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = e.Id;
        
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
        command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime;
        command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = endTime;
        command.Parameters.Add("@ExcludedId", SqlDbType.Int).Value = (object?)excludedId ?? DBNull.Value;
        
        var count = (int)command.ExecuteScalar();
        return count > 0;
    }
}
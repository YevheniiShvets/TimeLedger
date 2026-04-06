using Microsoft.Data.SqlClient;
using TimeLedger.Models;

namespace TimeLedger.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    
    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public User? GetById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, Email, PasswordHash, CreatedAt
        FROM Users
        WHERE Id = @Id";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            };
        }
        return null;
    }

    public User? GetByEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, Email, PasswordHash, CreatedAt
        FROM Users
        WHERE Email = @Email";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                CreatedAt = reader.GetDateTime(3)
            };
        }
        return null;
    }

    public User Add(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, CreatedAt)
            VALUES (@Email, @PasswordHash, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

        var result = command.ExecuteScalar();
        if (result == null)
            throw new Exception("Failed to insert user.");

        user.Id = Convert.ToInt32(result);
        return user;
    }

    public User Update(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            UPDATE Users
            SET Email = @Email,
            PasswordHash = @PasswordHash
            WHERE Id = @Id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

        var rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
            throw new Exception("Failed to update user.");

        return user;
    }

    public void Delete(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        const string sql = @"
        DELETE FROM Users
        WHERE Id = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.ExecuteNonQuery();
    }

    public bool Exists(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT COUNT(*)
        FROM Users
        WHERE Email = @Email";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);
        
        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }
}
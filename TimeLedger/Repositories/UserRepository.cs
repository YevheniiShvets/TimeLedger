using System.Data;
using Microsoft.Data.SqlClient;
using TimeLedger.Models;

namespace TimeLedger.Repositories;

public class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection is not working");

    public User? GetById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, Name, Email, PasswordHash, CreatedAt
        FROM Users
        WHERE Id = @Id";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
        return null;
    }

    public User? GetByEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        
        const string sql = @"
        SELECT Id, Name, Email, PasswordHash, CreatedAt
        FROM Users
        WHERE Email = @Email";
        
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = email;
        
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }
        return null;
    }

    public User Add(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            INSERT INTO Users (Name, Email, PasswordHash, CreatedAt)
            VALUES (@Name, @Email, @PasswordHash, @CreatedAt);
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = user.Name;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = user.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 1000).Value = user.PasswordHash;
        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = user.CreatedAt;

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
            SET Name = @Name,
            Email = @Email,
            PasswordHash = @PasswordHash
            WHERE Id = @Id";

        using var command = new SqlCommand(sql, connection);
        
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = user.Name;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = user.Id;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = user.Email;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 1000).Value = user.PasswordHash;

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
        command.Parameters.Add("@Id", SqlDbType.Int).Value = user.Id;
        var rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected == 0)
            throw new Exception("Failed to delete user.");
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
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = email;
        
        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }
}
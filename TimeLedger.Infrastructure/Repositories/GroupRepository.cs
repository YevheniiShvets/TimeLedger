using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Infrastructure.Repositories;

public class GroupRepository(IConfiguration configuration) : IGroupRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database is not working");

    public IEnumerable<Group> GetAllGroups(int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var groups = new List<Group>();

        const string sql = @"
        SELECT Id, OwnerId, Name
        FROM Groups
        WHERE OwnerId = @OwnerId
        ORDER BY Name";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            groups.Add(new Group
            {
                Id = reader.GetInt32(0),
                OwnerId = reader.GetInt32(1),
                Name = reader.GetString(2)
            });
        }
        return groups;
    }

    public Group GetGroupById(int id, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT Id, OwnerId, Name
        FROM Groups
        WHERE Id = @Id AND OwnerId = @OwnerId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Group
            {
                Id = reader.GetInt32(0),
                OwnerId = reader.GetInt32(1),
                Name = reader.GetString(2)
            };
        }
        throw new KeyNotFoundException($"Group with ID {id} not found.");
    }

    public Group CreateGroup(Group group)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        INSERT INTO Groups (OwnerId, Name)
        VALUES (@OwnerId, @Name);
        SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = group.OwnerId;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = group.Name;
        var id = Convert.ToInt32(command.ExecuteScalar());
        group.Id = id;
        return group;
    }

    public Group UpdateGroup(Group group)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        UPDATE Groups
        SET Name = @Name
        WHERE Id = @Id AND OwnerId = @OwnerId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = group.Name;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = group.Id;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = group.OwnerId;
        command.ExecuteNonQuery();
        return group;
    }

    public void DeleteGroup(int id, int ownerId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        DELETE FROM Groups
        WHERE Id = @Id AND OwnerId = @OwnerId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
        command.ExecuteNonQuery();
    }

    public IEnumerable<int> GetGroupMembers(int groupId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var userIds = new List<int>();

        const string sql = @"
        SELECT UserId
        FROM GroupMembers
        WHERE GroupId = @GroupId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            userIds.Add(reader.GetInt32(0));
        }
        return userIds;
    }

    public void AddGroupMember(int groupId, int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        INSERT INTO GroupMembers (GroupId, UserId)
        VALUES (@GroupId, @UserId)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.ExecuteNonQuery();
    }

    public void RemoveGroupMember(int groupId, int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        DELETE FROM GroupMembers
        WHERE GroupId = @GroupId AND UserId = @UserId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.ExecuteNonQuery();
    }

    public bool IsMember(int groupId, int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
        SELECT CASE
            WHEN EXISTS (
                SELECT 1
                FROM GroupMembers
                WHERE GroupId = @GroupId AND UserId = @UserId
            ) THEN 1
            ELSE 0
        END";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        var exists = Convert.ToInt32(command.ExecuteScalar());
        return exists == 1;
    }

}
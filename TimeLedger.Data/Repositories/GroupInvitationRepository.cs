using System.Data;
using BusinessCollaboration.Interfaces.Group;
using BusinessCollaboration.Models.Group;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TimeLedger.Infrastructure.Repositories;

public class GroupInvitationRepository(IConfiguration configuration) : IGroupInvitationRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Database is not working");
    public GroupInvitation Create(GroupInvitation invitation)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        const string sql = @"
        INSERT INTO GroupInvitations (GroupId, InvitedUserId, InvitedByUserId, CreatedAt, ExpiresAt)
        VALUES (@GroupId, @InvitedUserId, @InvitedByUserId, @CreatedAt, @ExpiresAt);
        SELECT SCOPE_IDENTITY();";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", SqlDbType.Int).Value = invitation.GroupId;
        command.Parameters.Add("@InvitedUserId", SqlDbType.Int).Value = invitation.InvitedUserId;
        command.Parameters.Add("@InvitedByUserId", SqlDbType.Int).Value = invitation.InvitedByUserId;
        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
        command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = (object?)invitation.ExpiresAt ?? DBNull.Value;
        
        var id = command.ExecuteScalar();
        invitation.Id = Convert.ToInt32(id);
        return invitation;
    }

    public GroupInvitation? GetById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        const string sql = @"
        SELECT Id, GroupId, InvitedUserId, InvitedByUserId, CreatedAt, ExpiresAt
        FROM GroupInvitations
        WHERE Id = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new GroupInvitation
            {
                Id = reader.GetInt32(0),
                GroupId = reader.GetInt32(1),
                InvitedUserId = reader.GetInt32(2),
                InvitedByUserId = reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                ExpiresAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            };
        }
        return null;
    }

    public IEnumerable<GroupInvitation> GetPendingInvitationsForUser(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var invitations = new List<GroupInvitation>();
        const string sql = @"
        SELECT Id, GroupId, InvitedUserId, InvitedByUserId, CreatedAt, ExpiresAt
        FROM GroupInvitations
        WHERE InvitedUserId = @UserId AND (ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE())";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            invitations.Add(new GroupInvitation
            {
                Id = reader.GetInt32(0),
                GroupId = reader.GetInt32(1),
                InvitedUserId = reader.GetInt32(2),
                InvitedByUserId = reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                ExpiresAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            });
        }
        return invitations;
    }

    public IEnumerable<GroupInvitation> GetByGroupId(int groupId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var invitations = new List<GroupInvitation>();
        const string sql = @"
        SELECT Id, GroupId, InvitedUserId, InvitedByUserId, CreatedAt, ExpiresAt
        FROM GroupInvitations
        WHERE GroupId = @GroupId";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@GroupId", SqlDbType.Int).Value = groupId;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            invitations.Add(new GroupInvitation
            {
                Id = reader.GetInt32(0),
                GroupId = reader.GetInt32(1),
                InvitedUserId = reader.GetInt32(2),
                InvitedByUserId = reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                ExpiresAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            });
        }
        return invitations;
    }

    public void Delete(int invitationId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        const string sql = @"
        DELETE FROM GroupInvitations
        WHERE Id = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = invitationId;
        command.ExecuteNonQuery();
    }
}
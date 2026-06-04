using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.Interfaces.Groups;
using TimeLedger.Core.Interfaces.Users;
using TimeLedger.Core.Models.Groups;
using TimeLedger.Core.Models.Users;
using TimeLedger.Core.Services;

namespace TimeLedger.Tests.Services.Inbox;

[TestFixture]
public class InboxServiceTests
{
    private IGroupInvitationRepository _invitationRepository = null!;
    private IUserRepository _userRepository = null!;
    private IGroupRepository _groupRepository = null!;
    private IGroupInvitationService _invitationService = null!;

    private const int ActorUserId = 10;
    private const int InvitedUserId = 11;
    private const int GroupId = 5;
    private const int InvitationId = 100;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static GroupInvitation MakeEntity(
        int id = InvitationId,
        int groupId = GroupId,
        int invitedUserId = InvitedUserId,
        int invitedByUserId = ActorUserId) => new()
    {
        Id = id,
        GroupId = groupId,
        InvitedUserId = invitedUserId,
        InvitedByUserId = invitedByUserId,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(30)
    };

    private static CreateGroupInvitationDto MakeInviteDto(
        string email = "invitee@example.com") => new()
    {
        InviteeEmail = email
    };

    private static User MakeUser(
        int id = ActorUserId,
        string email = "actor@example.com",
        string name = "Actor User") => new()
    {
        Id = id,
        Email = email,
        Name = name,
        PasswordHash = "hashed_password",
        CreatedAt = DateTime.UtcNow
    };

    private static Group MakeGroup(
        int id = GroupId,
        string name = "Test Group",
        int ownerId = ActorUserId) => new()
    {
        Id = id,
        Name = name,
        OwnerId = ownerId
    };

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _invitationRepository = Substitute.For<IGroupInvitationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _groupRepository = Substitute.For<IGroupRepository>();
        _invitationService = new GroupInvitationService(_invitationRepository, _userRepository, _groupRepository);
    }

    // =========================================================================
    // Invite — validation
    // =========================================================================

    [Test]
    public void Invite_Throws_ArgumentException_WhenInviteeEmailIsNull()
    {
        // Arrange
        var dto = MakeInviteDto();
        dto.InviteeEmail = null!;

        // Act & Assert
        Assert.That(() => _invitationService.Invite(GroupId, dto, ActorUserId),
            Throws.ArgumentException.With.Message.Contains("Invitee email is required"));
    }

    [Test]
    public void Invite_Throws_InvalidOperationException_WhenInviteeUserNotFound()
    {
        // Arrange
        var dto = MakeInviteDto(email: "nonexistent@example.com");
        _userRepository.GetByEmail(dto.InviteeEmail).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.Invite(GroupId, dto, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found"));
    }

    [Test]
    public void Invite_Throws_InvalidOperationException_WhenActorUserNotFound()
    {
        // Arrange
        var dto = MakeInviteDto();
        var inviteeUser = MakeUser(id: InvitedUserId, email: dto.InviteeEmail);
        _userRepository.GetByEmail(dto.InviteeEmail).Returns(inviteeUser);
        _userRepository.GetById(ActorUserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.Invite(GroupId, dto, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("Actor user not found"));
    }

    [Test]
    public void Invite_Throws_InvalidOperationException_WhenGroupNotFound()
    {
        // Arrange
        var dto = MakeInviteDto();
        var inviteeUser = MakeUser(id: InvitedUserId, email: dto.InviteeEmail);
        var actorUser = MakeUser(id: ActorUserId);
        _userRepository.GetByEmail(dto.InviteeEmail).Returns(inviteeUser);
        _userRepository.GetById(ActorUserId).Returns(actorUser);
        _groupRepository.GetGroupById(GroupId, ActorUserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.Invite(GroupId, dto, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("Group not found"));
    }

    [Test]
    public void Invite_Throws_InvalidOperationException_WhenActorInvitesSelf()
    {
        // Arrange
        var dto = MakeInviteDto(email: "actor@example.com");
        var user = MakeUser(id: ActorUserId, email: dto.InviteeEmail);
        var group = MakeGroup();
        _userRepository.GetByEmail(dto.InviteeEmail).Returns(user);
        _userRepository.GetById(ActorUserId).Returns(user);
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);

        // Act & Assert
        Assert.That(() => _invitationService.Invite(GroupId, dto, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("cannot invite themselves"));
    }

    [Test]
    public void Invite_Throws_InvalidOperationException_WhenPendingInvitationAlreadyExists()
    {
        // Arrange
        var dto = MakeInviteDto();
        var inviteeUser = MakeUser(id: InvitedUserId, email: dto.InviteeEmail);
        var actorUser = MakeUser(id: ActorUserId);
        var group = MakeGroup();
        var existingInvitation = MakeEntity();

        _userRepository.GetByEmail(dto.InviteeEmail).Returns(inviteeUser);
        _userRepository.GetById(ActorUserId).Returns(actorUser);
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);
        _invitationRepository.GetPendingInvitationsForUser(InvitedUserId).Returns([existingInvitation]);

        // Act & Assert
        Assert.That(() => _invitationService.Invite(GroupId, dto, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("already has a pending invitation"));
    }

    // =========================================================================
    // Invite — successful creation
    // =========================================================================

    [Test]
    public void Invite_CreateInvitation_WhenValidationPasses()
    {
        // Arrange
        var dto = MakeInviteDto();
        var inviteeUser = MakeUser(id: InvitedUserId, email: dto.InviteeEmail);
        var actorUser = MakeUser(id: ActorUserId, name: "Sender");
        var group = MakeGroup(name: "Dev Team");
        var invitation = MakeEntity();

        _userRepository.GetByEmail(dto.InviteeEmail).Returns(inviteeUser);
        _userRepository.GetById(ActorUserId).Returns(actorUser);
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);
        _invitationRepository.GetPendingInvitationsForUser(InvitedUserId).Returns([]);
        _invitationRepository.Create(Arg.Any<GroupInvitation>()).Returns(invitation);

        // Act
        var result = _invitationService.Invite(GroupId, dto, ActorUserId);

        // Assert
        Assert.That(result, Is.Not.Null);
        _invitationRepository.Received(1).Create(Arg.Is<GroupInvitation>(inv =>
            inv.InvitedUserId == InvitedUserId &&
            inv.GroupId == GroupId &&
            inv.InvitedByUserId == ActorUserId));
    }

    [Test]
    public void Invite_ReturnsInboxItemDto_WithCorrectMapping()
    {
        // Arrange
        var dto = MakeInviteDto();
        var inviteeUser = MakeUser(id: InvitedUserId, email: dto.InviteeEmail);
        var actorUser = MakeUser(id: ActorUserId, name: "Alice");
        var group = MakeGroup(name: "Developers");
        var invitation = MakeEntity(id: 77);

        _userRepository.GetByEmail(dto.InviteeEmail).Returns(inviteeUser);
        _userRepository.GetById(ActorUserId).Returns(actorUser);
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);
        _invitationRepository.GetPendingInvitationsForUser(InvitedUserId).Returns([]);
        _invitationRepository.Create(Arg.Any<GroupInvitation>()).Returns(invitation);

        // Act
        var result = _invitationService.Invite(GroupId, dto, ActorUserId);

        // Assert
        Assert.That(result.Id, Is.EqualTo(77));
        Assert.That(result.Type, Is.EqualTo("GroupInvitation"));
        Assert.That(result.ContextId, Is.EqualTo(GroupId));
        Assert.That(result.ContextName, Is.EqualTo("Developers"));
        Assert.That(result.SentBy, Is.EqualTo("Alice"));
        Assert.That(result.ExpiresAt, Is.Not.Null);
    }

    // =========================================================================
    // GetPendingInboxForUser
    // =========================================================================

    [Test]
    public void GetPendingInboxForUser_ReturnsEmptyList_WhenNoInvitations()
    {
        // Arrange
        _invitationRepository.GetPendingInvitationsForUser(InvitedUserId).Returns([]);

        // Act
        var result = _invitationService.GetPendingInboxForUser(InvitedUserId).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetPendingInboxForUser_ReturnsMappedInvitations_WithCorrectData()
    {
        // Arrange
        var inv1 = MakeEntity(id: 1); // default group id is used
        var inv2 = MakeEntity(id: 2, groupId: 10, invitedByUserId: 12);
        var actor1 = MakeUser(id: ActorUserId, name: "Alice");
        var actor2 = MakeUser(id: 12, name: "Bob");
        var group1 = MakeGroup(name: "Group1");
        var group2 = MakeGroup(id: 10, name: "Group2");

        _invitationRepository.GetPendingInvitationsForUser(InvitedUserId).Returns([inv1, inv2]);
        _userRepository.GetById(ActorUserId).Returns(actor1);
        _userRepository.GetById(12).Returns(actor2);
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group1);
        _groupRepository.GetGroupById(10, 12).Returns(group2);

        // Act
        var result = _invitationService.GetPendingInboxForUser(InvitedUserId).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].ContextName, Is.EqualTo("Group1"));
        Assert.That(result[0].SentBy, Is.EqualTo("Alice"));
        Assert.That(result[1].ContextName, Is.EqualTo("Group2"));
        Assert.That(result[1].SentBy, Is.EqualTo("Bob"));
    }

    [Test]
    public void GetPendingInboxForUser_UsesUnknown_WhenActorUserNotFound()
    {
        // Arrange
        var inv = MakeEntity();
        var group = MakeGroup();

        _invitationRepository.GetPendingInvitationsForUser(InvitedUserId).Returns([inv]);
        _userRepository.GetById(ActorUserId).ReturnsNull();
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);

        // Act
        var result = _invitationService.GetPendingInboxForUser(InvitedUserId).Single();

        // Assert
        Assert.That(result.SentBy, Is.EqualTo("Unknown"));
    }

    // =========================================================================
    // AcceptInvitation
    // =========================================================================

    [Test]
    public void AcceptInvitation_Throws_InvalidOperationException_WhenInvitationNotFound()
    {
        // Arrange
        _invitationRepository.GetById(InvitationId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.AcceptInvitation(InvitationId, InvitedUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void AcceptInvitation_Throws_InvalidOperationException_WhenAccessDenied()
    {
        // Arrange
        var invitation = MakeEntity(invitedUserId: 99); // Different user
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act & Assert
        Assert.That(() => _invitationService.AcceptInvitation(InvitationId, InvitedUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void AcceptInvitation_AddsMemberToGroup_AndDeletesInvitation()
    {
        // Arrange
        var invitation = MakeEntity();
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act
        _invitationService.AcceptInvitation(InvitationId, InvitedUserId);

        // Assert
        _groupRepository.Received(1).AddGroupMember(GroupId, InvitedUserId);
        _invitationRepository.Received(1).Delete(InvitationId);
    }

    // =========================================================================
    // DeclineInvitation
    // =========================================================================

    [Test]
    public void DeclineInvitation_Throws_InvalidOperationException_WhenInvitationNotFound()
    {
        // Arrange
        _invitationRepository.GetById(InvitationId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.DeclineInvitation(InvitationId, InvitedUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void DeclineInvitation_Throws_InvalidOperationException_WhenAccessDenied()
    {
        // Arrange
        var invitation = MakeEntity(invitedUserId: 99); // Different user
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act & Assert
        Assert.That(() => _invitationService.DeclineInvitation(InvitationId, InvitedUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void DeclineInvitation_DeletesInvitation_WhenValidationPasses()
    {
        // Arrange
        var invitation = MakeEntity();
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act
        _invitationService.DeclineInvitation(InvitationId, InvitedUserId);

        // Assert
        _invitationRepository.Received(1).Delete(InvitationId);
        _groupRepository.DidNotReceive().AddGroupMember(Arg.Any<int>(), Arg.Any<int>());
    }

    // =========================================================================
    // GetInvitation
    // =========================================================================

    [Test]
    public void GetInvitation_Throws_InvalidOperationException_WhenInvitationNotFound()
    {
        // Arrange
        _invitationRepository.GetById(InvitationId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.GetInvitation(InvitationId, InvitedUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void GetInvitation_Throws_InvalidOperationException_WhenAccessDenied()
    {
        // Arrange
        var invitation = MakeEntity(invitedUserId: 99); // Different user
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act & Assert
        Assert.That(() => _invitationService.GetInvitation(InvitationId, InvitedUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void GetInvitation_ReturnsInboxItemDto_WithCorrectMapping()
    {
        // Arrange
        var invitation = MakeEntity();
        var actorUser = MakeUser(id: ActorUserId, name: "Sender");
        var group = MakeGroup(name: "TestGroup");

        _invitationRepository.GetById(InvitationId).Returns(invitation);
        _userRepository.GetById(ActorUserId).Returns(actorUser);
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);

        // Act
        var result = _invitationService.GetInvitation(InvitationId, InvitedUserId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(InvitationId));
        Assert.That(result.Type, Is.EqualTo("GroupInvitation"));
        Assert.That(result.ContextName, Is.EqualTo("TestGroup"));
        Assert.That(result.SentBy, Is.EqualTo("Sender"));
    }

    [Test]
    public void GetInvitation_UsesUnknown_WhenActorUserNotFound()
    {
        // Arrange
        var invitation = MakeEntity();
        var group = MakeGroup();

        _invitationRepository.GetById(InvitationId).Returns(invitation);
        _userRepository.GetById(ActorUserId).ReturnsNull();
        _groupRepository.GetGroupById(GroupId, ActorUserId).Returns(group);

        // Act
        var result = _invitationService.GetInvitation(InvitationId, InvitedUserId);

        // Assert
        Assert.That(result.SentBy, Is.EqualTo("Unknown"));
    }

    // =========================================================================
    // DeleteInvitation
    // =========================================================================

    [Test]
    public void DeleteInvitation_Throws_InvalidOperationException_WhenInvitationNotFound()
    {
        // Arrange
        _invitationRepository.GetById(InvitationId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _invitationService.DeleteInvitation(InvitationId, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void DeleteInvitation_Throws_InvalidOperationException_WhenAccessDenied()
    {
        // Arrange - user is not the creator (InvitedByUserId)
        var invitation = MakeEntity(invitedByUserId: 99);
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act & Assert
        Assert.That(() => _invitationService.DeleteInvitation(InvitationId, ActorUserId),
            Throws.InvalidOperationException.With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void DeleteInvitation_DeletesInvitation_WhenActorIsCreator()
    {
        // Arrange
        var invitation = MakeEntity(invitedByUserId: ActorUserId);
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act
        _invitationService.DeleteInvitation(InvitationId, ActorUserId);

        // Assert
        _invitationRepository.Received(1).Delete(InvitationId);
    }

    [Test]
    public void DeleteInvitation_AllowsOnlyCreator_NotInvitee()
    {
        // Arrange - ActorUserId is the one who created the invitation
        var invitation = MakeEntity(invitedByUserId: ActorUserId, invitedUserId: 77);
        _invitationRepository.GetById(InvitationId).Returns(invitation);

        // Act
        _invitationService.DeleteInvitation(InvitationId, ActorUserId);

        // Assert
        _invitationRepository.Received(1).Delete(InvitationId);
    }
}
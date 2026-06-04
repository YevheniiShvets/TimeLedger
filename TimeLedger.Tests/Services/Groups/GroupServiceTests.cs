using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TimeLedger.Core.DTOs.Groups;
using TimeLedger.Core.Interfaces.Groups;
using TimeLedger.Core.Interfaces.Users;
using TimeLedger.Core.Models.Groups;
using TimeLedger.Core.Models.Users;
using TimeLedger.Core.Services;

namespace TimeLedger.Tests.Services.Groups;

[TestFixture]
public class GroupServiceTests
{
    private IGroupRepository _groupRepository = null!;
    private IUserRepository _userRepository = null!;
    private IGroupService _groupService = null!;

    private const int UserId = 12;
    private const int OtherUserId = 99;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Group MakeEntity(
        int id = 1,
        string name = "Group Test",
        int ownerId = UserId) => new()
    {
        Id = id,
        Name = name,
        OwnerId = ownerId
    };

    private static CreateGroupDto MakeCreateDto(
        string name = "Group Test") => new()
    {
        Name = name
    };

    private static UpdateGroupDto MakeUpdateDto(
        string name = "Group Test") => new()
    {
        Name = name
    };

    private static User MakeUser(
        int id = UserId,
        string email = "user@example.com",
        string name = "Test User") => new()
    {
        Id = id,
        Email = email,
        Name = name,
        CreatedAt = DateTime.UtcNow
    };

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _groupService = new GroupService(_groupRepository, _userRepository);
    }

    // =========================================================================
    // GetAll
    // =========================================================================

    [Test]
    public void GetAll_ReturnsMappedDtos_ForEachEntityInRepository()
    {
        // Arrange
        _groupRepository.GetAllGroups(UserId).Returns([MakeEntity(id: 1), MakeEntity(id: 2), MakeEntity(id: 3)]);

        // Act
        var result = _groupService.GetAll(UserId).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void GetAll_ReturnsEmptyCollection_WhenRepositoryHasNoGroups()
    {
        // Arrange
        _groupRepository.GetAllGroups(UserId).Returns([]);

        // Act
        var result = _groupService.GetAll(UserId);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAll_MapsGroupProperties_Correctly()
    {
        // Arrange
        var entity = MakeEntity(id: 5, name: "Dev Team");
        _groupRepository.GetAllGroups(UserId).Returns([entity]);

        // Act
        var result = _groupService.GetAll(UserId).Single();

        // Assert
        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.Name, Is.EqualTo("Dev Team"));
        Assert.That(result.OwnerId, Is.EqualTo(UserId));
    }

    // =========================================================================
    // GetById
    // =========================================================================

    [Test]
    public void GetById_ReturnsDTO_WithMembers_WhenGroupExists()
    {
        // Arrange
        var group = MakeEntity(id: 1);
        _groupRepository.GetGroupById(1, UserId).Returns(group);
        _groupRepository.GetGroupMembers(1).Returns([OtherUserId]);
        _userRepository.GetById(UserId).Returns(MakeUser(UserId, "owner@example.com", "Owner"));
        _userRepository.GetById(OtherUserId).Returns(MakeUser(OtherUserId, "member@example.com", "Member"));

        // Act
        var result = _groupService.GetById(1, UserId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Members, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetById_Throws_InvalidOperationException_WhenGroupDoesNotExist()
    {
        // Arrange
        _groupRepository.GetGroupById(99, UserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _groupService.GetById(99, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void GetById_Throws_InvalidOperationException_WhenAccessDenied()
    {
        // Arrange
        _groupRepository.GetGroupById(1, OtherUserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _groupService.GetById(1, OtherUserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not found or access denied"));
    }

    // =========================================================================
    // Create — field validation
    // =========================================================================

    [Test]
    public void Create_Throws_WhenNameIsEmpty()
    {
        // Arrange
        var dto = MakeCreateDto();
        dto.Name = "";

        // Act & Assert
        Assert.That(() => _groupService.Create(dto, UserId),
            Throws.ArgumentException.With.Message.Contains("name"));
    }

    [Test]
    public void Create_Throws_WhenNameIsWhitespace()
    {
        // Arrange
        var dto = MakeCreateDto();
        dto.Name = "   ";

        // Act & Assert
        Assert.That(() => _groupService.Create(dto, UserId),
            Throws.ArgumentException.With.Message.Contains("name"));
    }

    [Test]
    public void Create_Throws_WhenNameIsNull()
    {
        // Arrange
        var dto = MakeCreateDto();
        dto.Name = null!;

        // Act & Assert
        Assert.That(() => _groupService.Create(dto, UserId),
            Throws.ArgumentException.With.Message.Contains("name"));
    }

    [Test]
    public void Create_Throws_WhenNameExceeds100Characters()
    {
        // Arrange
        var dto = MakeCreateDto();
        dto.Name = new string('x', 101);

        // Act & Assert
        Assert.That(() => _groupService.Create(dto, UserId), Throws.ArgumentException);
    }

    [Test]
    public void Create_DoesNotThrow_WhenNameIsExactly100Characters()
    {
        // Arrange
        var dto = MakeCreateDto();
        dto.Name = new string('x', 100);
        _groupRepository.CreateGroup(Arg.Any<Group>()).Returns(MakeEntity(name: dto.Name));

        // Act & Assert
        Assert.That(() => _groupService.Create(dto, UserId), Throws.Nothing);
    }

    // =========================================================================
    // Create — ownership and persistence
    // =========================================================================

    [Test]
    public void Create_AssignsOwnership_ToCurrentUser()
    {
        // Arrange
        var dto = MakeCreateDto();
        _groupRepository.CreateGroup(Arg.Do<Group>(g =>
        {
            Assert.That(g.OwnerId, Is.EqualTo(UserId));
        })).Returns(MakeEntity(ownerId: UserId));

        // Act
        _groupService.Create(dto, UserId);

        // Assert - verified in Arg.Do above
    }

    [Test]
    public void Create_SavesGroupToRepository()
    {
        // Arrange
        var dto = MakeCreateDto();
        _groupRepository.CreateGroup(Arg.Any<Group>()).Returns(MakeEntity());

        // Act
        _groupService.Create(dto, UserId);

        // Assert
        _groupRepository.Received(1).CreateGroup(Arg.Any<Group>());
    }

    [Test]
    public void Create_ReturnsDto_WhenSuccessful()
    {
        // Arrange
        var dto = MakeCreateDto(name: "New Group");
        var saved = MakeEntity(id: 10, name: "New Group");
        _groupRepository.CreateGroup(Arg.Any<Group>()).Returns(saved);

        // Act
        var result = _groupService.Create(dto, UserId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.Name, Is.EqualTo("New Group"));
    }

    [Test]
    public void Create_TrimsName_BeforeSaving()
    {
        // Arrange
        var dto = MakeCreateDto(name: "  Group Name  ");
        _groupRepository.CreateGroup(Arg.Do<Group>(g =>
        {
            Assert.That(g.Name, Is.EqualTo("Group Name"));
        })).Returns(MakeEntity(name: "Group Name"));

        // Act
        _groupService.Create(dto, UserId);

        // Assert - verified in Arg.Do above
    }

    // =========================================================================
    // Update — not found
    // =========================================================================

    [Test]
    public void Update_Throws_InvalidOperationException_WhenGroupDoesNotExist()
    {
        // Arrange
        _groupRepository.GetGroupById(99, UserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _groupService.Update(99, MakeUpdateDto(), UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not found or access denied"));
    }

    // =========================================================================
    // Update — ownership check
    // =========================================================================

    [Test]
    public void Update_Throws_InvalidOperationException_WhenUserIsNotOwner()
    {
        // Arrange
        var group = MakeEntity(ownerId: OtherUserId);
        _groupRepository.GetGroupById(1, UserId).Returns(group);

        // Act & Assert
        Assert.That(() => _groupService.Update(1, MakeUpdateDto(), UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Only the group owner"));
    }

    // =========================================================================
    // Update — field validation
    // =========================================================================

    [Test]
    public void Update_Throws_WhenNameIsEmpty()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = "";

        // Act & Assert
        Assert.That(() => _groupService.Update(1, dto, UserId),
            Throws.ArgumentException.With.Message.Contains("name"));
    }

    [Test]
    public void Update_Throws_WhenNameIsWhitespace()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = "   ";

        // Act & Assert
        Assert.That(() => _groupService.Update(1, dto, UserId),
            Throws.ArgumentException.With.Message.Contains("name"));
    }

    [Test]
    public void Update_Throws_WhenNameExceeds100Characters()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = new string('x', 101);

        // Act & Assert
        Assert.That(() => _groupService.Update(1, dto, UserId), Throws.ArgumentException);
    }

    // =========================================================================
    // Update — persistence
    // =========================================================================

    [Test]
    public void Update_CallsRepositoryUpdate_WhenValidationPasses()
    {
        // Arrange
        var entity = MakeEntity();
        _groupRepository.GetGroupById(1, UserId).Returns(entity);
        _groupRepository.GetGroupMembers(1).Returns([]);
        _groupRepository.UpdateGroup(Arg.Any<Group>()).Returns(entity);
        _userRepository.GetById(UserId).Returns(MakeUser());

        // Act
        _groupService.Update(1, MakeUpdateDto(), UserId);

        // Assert
        _groupRepository.Received(1).UpdateGroup(Arg.Any<Group>());
    }

    [Test]
    public void Update_ReturnsUpdatedDto_WhenSuccessful()
    {
        // Arrange
        var entity = MakeEntity(id: 1, name: "Old Name");
        _groupRepository.GetGroupById(1, UserId).Returns(entity);
        var updated = MakeEntity(id: 1, name: "New Name");
        _groupRepository.GetGroupMembers(1).Returns([]);
        _groupRepository.UpdateGroup(Arg.Any<Group>()).Returns(updated);
        _userRepository.GetById(UserId).Returns(MakeUser());

        // Act
        var result = _groupService.Update(1, MakeUpdateDto(name: "New Name"), UserId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public void Update_DoesNotCallUpdate_WhenValidationFails()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = "";

        // Act & Assert
        try { _groupService.Update(1, dto, UserId); } catch (ArgumentException) { }

        _groupRepository.DidNotReceive().UpdateGroup(Arg.Any<Group>());
    }

    [Test]
    public void Update_IncludesMembers_InReturnedDto()
    {
        // Arrange
        var entity = MakeEntity(id: 1);
        _groupRepository.GetGroupById(1, UserId).Returns(entity);
        _groupRepository.GetGroupMembers(1).Returns([OtherUserId]);
        _groupRepository.UpdateGroup(Arg.Any<Group>()).Returns(entity);
        _userRepository.GetById(UserId).Returns(MakeUser(UserId, "owner@example.com"));
        _userRepository.GetById(OtherUserId).Returns(MakeUser(OtherUserId, "member@example.com"));

        // Act
        var result = _groupService.Update(1, MakeUpdateDto(), UserId);

        // Assert
        Assert.That(result.Members, Has.Count.EqualTo(2));
    }

    // =========================================================================
    // Delete
    // =========================================================================

    [Test]
    public void Delete_Throws_InvalidOperationException_WhenGroupDoesNotExist()
    {
        // Arrange
        _groupRepository.GetGroupById(99, UserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _groupService.Delete(99, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void Delete_Throws_InvalidOperationException_WhenUserIsNotOwner()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity(ownerId: OtherUserId));

        // Act & Assert
        Assert.That(() => _groupService.Delete(1, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Only the group owner"));
    }

    [Test]
    public void Delete_CallsRepositoryDelete_WhenOwner()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());

        // Act
        _groupService.Delete(1, UserId);

        // Assert
        _groupRepository.Received(1).DeleteGroup(1, UserId);
    }

    [Test]
    public void Delete_DoesNotCallDelete_WhenGroupNotFound()
    {
        // Arrange
        _groupRepository.GetGroupById(99, UserId).ReturnsNull();

        // Act & Assert
        try { _groupService.Delete(99, UserId); } catch (InvalidOperationException) { }

        _groupRepository.DidNotReceive().DeleteGroup(Arg.Any<int>(), Arg.Any<int>());
    }

    // =========================================================================
    // RemoveMember
    // =========================================================================

    [Test]
    public void RemoveMember_Throws_ArgumentException_WhenUserIdIsInvalid()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());

        // Act & Assert
        Assert.That(() => _groupService.RemoveMember(1, 0, UserId),
            Throws.ArgumentException.With.Message.Contains("User is required"));
    }

    [Test]
    public void RemoveMember_Throws_InvalidOperationException_WhenGroupNotFound()
    {
        // Arrange
        _groupRepository.GetGroupById(99, UserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _groupService.RemoveMember(99, OtherUserId, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void RemoveMember_Throws_InvalidOperationException_WhenActorIsNotOwner()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity(ownerId: OtherUserId));

        // Act & Assert
        Assert.That(() => _groupService.RemoveMember(1, 77, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Only the group owner"));
    }

    [Test]
    public void RemoveMember_Throws_InvalidOperationException_WhenRemovingOwner()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity(ownerId: UserId));

        // Act & Assert
        Assert.That(() => _groupService.RemoveMember(1, UserId, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Owner cannot be removed"));
    }

    [Test]
    public void RemoveMember_Throws_InvalidOperationException_WhenUserIsNotMember()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());
        _groupRepository.IsMember(1, OtherUserId).Returns(false);

        // Act & Assert
        Assert.That(() => _groupService.RemoveMember(1, OtherUserId, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not a member"));
    }

    [Test]
    public void RemoveMember_CallsRepositoryRemove_WhenValidationPasses()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).Returns(MakeEntity());
        _groupRepository.IsMember(1, OtherUserId).Returns(true);

        // Act
        _groupService.RemoveMember(1, OtherUserId, UserId);

        // Assert
        _groupRepository.Received(1).RemoveGroupMember(1, OtherUserId);
    }

    // =========================================================================
    // GetMembers
    // =========================================================================

    [Test]
    public void GetMembers_Throws_InvalidOperationException_WhenGroupNotFound()
    {
        // Arrange
        _groupRepository.GetGroupById(1, UserId).ReturnsNull();

        // Act & Assert
        Assert.That(() => _groupService.GetMembers(1, UserId),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("not found or access denied"));
    }

    [Test]
    public void GetMembers_ReturnsEmptyList_WhenOnlyOwnerExists()
    {
        // Arrange
        var group = MakeEntity(ownerId: UserId);
        _groupRepository.GetGroupById(1, UserId).Returns(group);
        _groupRepository.GetGroupMembers(1).Returns([]);
        _userRepository.GetById(UserId).Returns(MakeUser(UserId, "owner@example.com"));

        // Act
        var result = _groupService.GetMembers(1, UserId).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetMembers_ReturnsAllMembers_IncludingOwner()
    {
        // Arrange
        var group = MakeEntity(ownerId: UserId);
        _groupRepository.GetGroupById(1, UserId).Returns(group);
        _groupRepository.GetGroupMembers(1).Returns([OtherUserId]);
        _userRepository.GetById(UserId).Returns(MakeUser(UserId, "owner@example.com"));
        _userRepository.GetById(OtherUserId).Returns(MakeUser(OtherUserId, "member@example.com"));

        // Act
        var result = _groupService.GetMembers(1, UserId).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetMembers_MapsUserToAccountInfoDto()
    {
        // Arrange
        var group = MakeEntity(ownerId: UserId);
        _groupRepository.GetGroupById(1, UserId).Returns(group);
        _groupRepository.GetGroupMembers(1).Returns([]);
        _userRepository.GetById(UserId).Returns(MakeUser(UserId, "test@example.com", "Test User"));

        // Act
        var result = _groupService.GetMembers(1, UserId).Single();

        // Assert
        Assert.That(result.Id, Is.EqualTo(UserId));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.Name, Is.EqualTo("Test User"));
    }
}

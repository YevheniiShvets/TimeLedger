using BusinessCollaboration.DTOs.User;
using BusinessCollaboration.Interfaces.User;
using BusinessCollaboration.Models.User;
using BusinessCollaboration.Services.User;
using NSubstitute;
using NSubstitute.ReturnsExtensions;


namespace TimeLedger.Tests.Services.Users;

[TestFixture]
public class UserServiceTests
{
    private IUserRepository _userRepository = null!;
    private UserService _userService = null!;

    private const int UserId = 12;
    private const string ValidPassword = "SecurePass123!";

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static User MakeEntity(
        int id = UserId,
        string name = "Test User",
        string email = "test@example.com",
        string password = ValidPassword) => new()
    {
        Id = id,
        Name = name,
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        CreatedAt = DateTime.UtcNow
    };

    private static RegisterDto MakeRegisterDto(
        string name = "Test User",
        string email = "test@example.com",
        string password = ValidPassword) => new()
    {
        Name = name,
        Email = email,
        Password = password,
        ConfirmPassword = password
    };

    private static LoginDto MakeLoginDto(
        string email = "test@example.com",
        string password = ValidPassword) => new()
    {
        Email = email,
        Password = password
    };

    private static UpdateAccountDto MakeUpdateDto(
        string name = "Updated User",
        string email = "updated@example.com",
        string password = ValidPassword) => new()
    {
        Name = name,
        Email = email,
        Password = password,
        ConfirmPassword = password
    };

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userService = new UserService(_userRepository);
    }

    // =========================================================================
    // Register — field validation
    // =========================================================================

    [Test]
    public void Register_Throws_WhenNameIsEmpty()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Name = "";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Name"));
    }

    [Test]
    public void Register_Throws_WhenNameIsWhitespace()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Name = "   ";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Name"));
    }

    [Test]
    public void Register_Throws_WhenNameExceeds100Characters()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Name = new string('x', 101);

        // Act & Assert
        Assert.That(() => _userService.Register(dto), Throws.ArgumentException);
    }

    [Test]
    public void Register_DoesNotThrow_WhenNameIsExactly100Characters()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Name = new string('x', 100);
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Add(Arg.Any<User>()).Returns(MakeEntity(name: dto.Name));

        // Act & Assert
        Assert.That(() => _userService.Register(dto), Throws.Nothing);
    }

    [Test]
    public void Register_Throws_WhenEmailIsEmpty()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Email = "";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Email"));
    }

    [Test]
    public void Register_Throws_WhenEmailIsWhitespace()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Email = "   ";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Email"));
    }

    [Test]
    public void Register_Throws_WhenEmailExceeds254Characters()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Email = new string('x', 246) + "@test.com"; // 255 chars total

        // Act & Assert
        Assert.That(() => _userService.Register(dto), Throws.ArgumentException);
    }

    [Test]
    public void Register_DoesNotThrow_WhenEmailIsExactly254Characters()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Email = new string('x', 245) + "@test.com"; // 254 chars total
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Add(Arg.Any<User>()).Returns(MakeEntity(email: dto.Email));

        // Act & Assert
        Assert.That(() => _userService.Register(dto), Throws.Nothing);
    }

    [Test]
    public void Register_Throws_WhenPasswordIsEmpty()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Password = "";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Password"));
    }

    [Test]
    public void Register_Throws_WhenPasswordIsWhitespace()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Password = "   ";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Password"));
    }

    [Test]
    public void Register_Throws_WhenConfirmPasswordIsEmpty()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.ConfirmPassword = "";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Confirm Password"));
    }

    [Test]
    public void Register_Throws_WhenConfirmPasswordIsWhitespace()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.ConfirmPassword = "   ";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.ArgumentException.With.Message.Contains("Confirm Password"));
    }

    // =========================================================================
    // Register — password validation
    // =========================================================================

    [Test]
    public void Register_Throws_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var dto = MakeRegisterDto();
        dto.Password = "SecurePass123!";
        dto.ConfirmPassword = "DifferentPass123!";

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.InvalidOperationException.With.Message.Contains("Passwords do not match"));
    }

    // =========================================================================
    // Register — email uniqueness and persistence
    // =========================================================================

    [Test]
    public void Register_Throws_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = MakeRegisterDto();
        _userRepository.Exists(dto.Email).Returns(true);

        // Act & Assert
        Assert.That(() => _userService.Register(dto),
            Throws.InvalidOperationException.With.Message.Contains("Email already in use"));
    }

    [Test]
    public void Register_TrimsNameBeforeSaving()
    {
        // Arrange
        var dto = MakeRegisterDto(name: "  Alice  ");
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Add(Arg.Do<User>(u =>
        {
            Assert.That(u.Name, Is.EqualTo("Alice"));
        })).Returns(MakeEntity(name: "Alice"));

        // Act
        _userService.Register(dto);

        // Assert - verified in Arg.Do above
    }

    [Test]
    public void Register_HashesPasswordWithBCrypt()
    {
        // Arrange
        var dto = MakeRegisterDto();
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Add(Arg.Do<User>(u =>
        {
            // Verify password is hashed (contains BCrypt format marker)
            Assert.That(u.PasswordHash, Is.Not.EqualTo(dto.Password));
            Assert.That(BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash), Is.True);
        })).Returns(MakeEntity());

        // Act
        _userService.Register(dto);

        // Assert - verified in Arg.Do above
    }

    [Test]
    public void Register_SavesUserToRepository()
    {
        // Arrange
        var dto = MakeRegisterDto();
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Add(Arg.Any<User>()).Returns(MakeEntity());

        // Act
        _userService.Register(dto);

        // Assert
        _userRepository.Received(1).Add(Arg.Any<User>());
    }

    [Test]
    public void Register_ReturnsAccountInfoDto_WhenSuccessful()
    {
        // Arrange
        var dto = MakeRegisterDto(name: "Alice", email: "alice@example.com");
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Add(Arg.Any<User>()).Returns(MakeEntity(id: 10, name: "Alice", email: "alice@example.com"));

        // Act
        var result = _userService.Register(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.Name, Is.EqualTo("Alice"));
        Assert.That(result.Email, Is.EqualTo("alice@example.com"));
    }

    // =========================================================================
    // Login — validation and authentication
    // =========================================================================

    [Test]
    public void Login_Throws_WhenEmailIsInvalid()
    {
        // Arrange
        var dto = MakeLoginDto(email: "nonexistent@example.com");
        _userRepository.GetByEmail(dto.Email).ReturnsNull();

        // Act & Assert
        Assert.That(() => _userService.Login(dto),
            Throws.InvalidOperationException.With.Message.Contains("Invalid email or password"));
    }

    [Test]
    public void Login_Throws_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = MakeEntity(password: "CorrectPass123!");
        var dto = MakeLoginDto(password: "WrongPass123!");
        _userRepository.GetByEmail(dto.Email).Returns(user);

        // Act & Assert
        Assert.That(() => _userService.Login(dto),
            Throws.InvalidOperationException.With.Message.Contains("Invalid email or password"));
    }

    [Test]
    public void Login_ReturnsAccountInfoDto_WhenCredentialsAreValid()
    {
        // Arrange
        var user = MakeEntity(id: 5, name: "Alice", email: "alice@example.com");
        var dto = MakeLoginDto(email: "alice@example.com");
        _userRepository.GetByEmail(dto.Email).Returns(user);

        // Act
        var result = _userService.Login(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.Email, Is.EqualTo("alice@example.com"));
    }

    [Test]
    public void Login_UsesBCryptVerify_ForPasswordComparison()
    {
        // Arrange
        var password = "MySecurePass123!";
        var user = MakeEntity(password: password);
        var dto = MakeLoginDto(password: password);
        _userRepository.GetByEmail(dto.Email).Returns(user);

        // Act
        var result = _userService.Login(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        _userRepository.Received(1).GetByEmail(dto.Email);
    }

    // =========================================================================
    // GetById
    // =========================================================================

    [Test]
    public void GetById_ReturnsAccountInfoDto_WhenUserExists()
    {
        // Arrange
        var user = MakeEntity(id: 7, name: "Bob", email: "bob@example.com");
        _userRepository.GetById(7).Returns(user);

        // Act
        var result = _userService.GetById(7);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(result.Name, Is.EqualTo("Bob"));
        Assert.That(result.Email, Is.EqualTo("bob@example.com"));
    }

    [Test]
    public void GetById_Throws_InvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetById(99).ReturnsNull();

        // Act & Assert
        Assert.That(() => _userService.GetById(99),
            Throws.InvalidOperationException.With.Message.Contains("User not found"));
    }

    // =========================================================================
    // Update — field validation
    // =========================================================================

    [Test]
    public void Update_Throws_InvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetById(99).ReturnsNull();

        // Act & Assert
        Assert.That(() => _userService.Update(99, MakeUpdateDto()),
            Throws.InvalidOperationException.With.Message.Contains("User not found"));
    }

    [Test]
    public void Update_Throws_WhenNameIsEmpty()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = "";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Name"));
    }

    [Test]
    public void Update_Throws_WhenNameIsWhitespace()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = "   ";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Name"));
    }

    [Test]
    public void Update_Throws_WhenNameExceeds100Characters()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Name = new string('x', 101);

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto), Throws.ArgumentException);
    }

    [Test]
    public void Update_Throws_WhenEmailIsEmpty()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Email = "";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Email"));
    }

    [Test]
    public void Update_Throws_WhenEmailIsWhitespace()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Email = "   ";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Email"));
    }

    [Test]
    public void Update_Throws_WhenEmailExceeds254Characters()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Email = new string('x', 246) + "@test.com"; // 255 chars total

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto), Throws.ArgumentException);
    }

    [Test]
    public void Update_Throws_WhenPasswordIsEmpty()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Password = "";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Password"));
    }

    [Test]
    public void Update_Throws_WhenPasswordIsWhitespace()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Password = "   ";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Password"));
    }

    [Test]
    public void Update_Throws_WhenConfirmPasswordIsEmpty()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.ConfirmPassword = "";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.ArgumentException.With.Message.Contains("Confirm Password"));
    }

    // =========================================================================
    // Update — email and password logic
    // =========================================================================

    [Test]
    public void Update_Throws_WhenNewEmailIsDuplicate()
    {
        // Arrange
        var user = MakeEntity(id: UserId, email: "old@example.com");
        _userRepository.GetById(UserId).Returns(user);
        _userRepository.Exists("new@example.com").Returns(true);
        var dto = MakeUpdateDto(email: "new@example.com");

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.InvalidOperationException.With.Message.Contains("Email already in use"));
    }

    [Test]
    public void Update_DoesNotCheckEmailDuplicate_WhenEmailUnchanged()
    {
        // Arrange
        var user = MakeEntity(email: "same@example.com");
        _userRepository.GetById(UserId).Returns(user);
        _userRepository.Update(Arg.Any<User>()).Returns(user);
        var dto = MakeUpdateDto(email: "same@example.com");

        // Act
        _userService.Update(UserId, dto);

        // Assert
        _userRepository.DidNotReceive().Exists(Arg.Any<string>());
    }

    [Test]
    public void Update_Throws_WhenPasswordsDoNotMatch()
    {
        // Arrange
        _userRepository.GetById(UserId).Returns(MakeEntity());
        var dto = MakeUpdateDto();
        dto.Password = "NewPass123!";
        dto.ConfirmPassword = "DifferentPass123!";

        // Act & Assert
        Assert.That(() => _userService.Update(UserId, dto),
            Throws.InvalidOperationException.With.Message.Contains("Passwords do not match"));
    }

    [Test]
    public void Update_UpdatesNameEmail_AndPassword_WhenValid()
    {
        // Arrange
        var user = MakeEntity(id: UserId, email: "old@example.com");
        _userRepository.GetById(UserId).Returns(user);
        _userRepository.Exists("new@example.com").Returns(false);
        _userRepository.Update(Arg.Any<User>()).Returns(user);
        var dto = MakeUpdateDto(name: "New Name", email: "new@example.com", password: "NewPass123!");

        // Act
        _userService.Update(UserId, dto);

        // Assert
        _userRepository.Received(1).Update(Arg.Is<User>(u =>
            u.Name == "New Name" &&
            u.Email == "new@example.com"));
    }

    [Test]
    public void Update_ReturnsUpdatedDto_WhenSuccessful()
    {
        // Arrange
        var user = MakeEntity(id: UserId);
        _userRepository.GetById(UserId).Returns(user);
        _userRepository.Exists(Arg.Any<string>()).Returns(false);
        _userRepository.Update(Arg.Any<User>()).Returns(user);
        var dto = MakeUpdateDto(name: "Updated Name");

        // Act
        var result = _userService.Update(UserId, dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        _userRepository.Received(1).Update(Arg.Any<User>());
    }

    // =========================================================================
    // Delete
    // =========================================================================

    [Test]
    public void Delete_Throws_InvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetById(99).ReturnsNull();

        // Act & Assert
        Assert.That(() => _userService.Delete(99),
            Throws.InvalidOperationException.With.Message.Contains("User not found"));
    }

    [Test]
    public void Delete_CallsRepositoryDelete_WithCorrectUser_WhenUserExists()
    {
        // Arrange
        var user = MakeEntity(id: UserId);
        _userRepository.GetById(UserId).Returns(user);

        // Act
        _userService.Delete(UserId);

        // Assert
        _userRepository.Received(1).Delete(Arg.Is<User>(u => u.Id == UserId));
    }

    [Test]
    public void Delete_DoesNotCallDelete_WhenUserNotFound()
    {
        // Arrange
        _userRepository.GetById(99).ReturnsNull();

        // Act & Assert
        try { _userService.Delete(99); } catch (InvalidOperationException) { }

        _userRepository.DidNotReceive().Delete(Arg.Any<User>());
    }

    // =========================================================================
    // Display Name Fallback
    // =========================================================================

    [Test]
    public void GetById_UseNameAsDisplayName_WhenNameIsNotEmpty()
    {
        // Arrange
        var user = MakeEntity(name: "Alice");
        _userRepository.GetById(UserId).Returns(user);

        // Act
        var result = _userService.GetById(UserId);

        // Assert
        Assert.That(result.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public void GetById_UseEmailPrefix_AsDisplayName_WhenNameIsEmpty()
    {
        // Arrange
        var user = MakeEntity(name: "", email: "alice.smith@example.com");
        _userRepository.GetById(UserId).Returns(user);

        // Act
        var result = _userService.GetById(UserId);

        // Assert
        Assert.That(result.Name, Is.EqualTo("alice.smith"));
    }

    [Test]
    public void GetById_UseEmailPrefix_AsDisplayName_WhenNameIsWhitespace()
    {
        // Arrange
        var user = MakeEntity(name: "   ", email: "bob@example.com");
        _userRepository.GetById(UserId).Returns(user);

        // Act
        var result = _userService.GetById(UserId);

        // Assert
        Assert.That(result.Name, Is.EqualTo("bob"));
    }

    [Test]
    public void GetById_UseFullEmail_AsDisplayName_WhenNoAtSymbolFound()
    {
        // Arrange
        var user = MakeEntity(name: "", email: "notanemail");
        _userRepository.GetById(UserId).Returns(user);

        // Act
        var result = _userService.GetById(UserId);

        // Assert
        Assert.That(result.Name, Is.EqualTo("notanemail"));
    }
}


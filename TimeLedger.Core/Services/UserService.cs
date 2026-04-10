using TimeLedger.Core.DTOs;
using TimeLedger.Core.Interfaces;
using TimeLedger.Core.Models;

namespace TimeLedger.Core.Services;

public class UserService(IUserRepository userRepository)
{
    public AccountInfoDto Register(RegisterDto dto)
    {
        ValidateRequired(dto.Name, dto.Email, dto.Password, dto.ConfirmPassword);
        ValidateLength(dto.Name, dto.Email);

        if (userRepository.Exists(dto.Email))
            throw new InvalidOperationException("Email already in use");

        if (dto.Password != dto.ConfirmPassword)
            throw new InvalidOperationException("Passwords do not match");

        var user = userRepository.Add(ToEntity(dto));
        return Map(user);
    }

    public AccountInfoDto Login(LoginDto dto)
    {
        var user = userRepository.GetByEmail(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid email or password");

        return Map(user);
    }

    public AccountInfoDto GetById(int id)
    {
        var user = userRepository.GetById(id);
        return user == null ? throw new InvalidOperationException("User not found") : Map(user);
    }

    public AccountInfoDto Update(int id, UpdateAccountDto dto)
    {
        var user = userRepository.GetById(id);
        if (user == null)
            throw new InvalidOperationException("User not found");
        
        ValidateRequired(dto.Name, dto.Email, dto.Password, dto.ConfirmPassword);
        ValidateLength(dto.Name, dto.Email);
        
        user.Name = dto.Name.Trim();
        
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (userRepository.Exists(dto.Email))
                throw new InvalidOperationException("Email already in use");

            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        return Map(userRepository.Update(user));
    }

    public void Delete(int id)
    {
        var user = userRepository.GetById(id);
        if (user == null)
            throw new InvalidOperationException("User not found");

        userRepository.Delete(user);
    }
    
    
    // Private helpers
    
    private static void ValidateRequired(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.");
        if  (string.IsNullOrWhiteSpace(confirmPassword))
            throw new ArgumentException("Confirm Password is required.");
    }
    private static void ValidateLength(string name, string email)
    {
        if (name.Trim().Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.");
        if (email.Length > 254)
            throw new ArgumentException("Email cannot exceed 254 characters.");
    }

    private static AccountInfoDto Map(User u) => new()
    {
        Id = u.Id,
        Name = GetDisplayName(u),
        Email = u.Email,
        CreatedAt = u.CreatedAt
    };

    private static User ToEntity(RegisterDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        CreatedAt = DateTime.UtcNow
    };

    private static string GetDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.Name))
            return user.Name;

        var atIndex = user.Email.IndexOf('@');
        return atIndex > 0 ? user.Email[..atIndex] : user.Email;
    }
}
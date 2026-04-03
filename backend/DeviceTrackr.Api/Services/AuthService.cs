using System.Security.Cryptography;
using System.Text;
using DeviceTrackr.Api.Dtos;
using DeviceTrackr.Api.Models;
using DeviceTrackr.Api.Repositories;

namespace DeviceTrackr.Api.Services;

public class AuthService(UserRepository users)
{
    public (bool Ok, string Error, AuthResponseDto? Data) Register(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Role) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return (false, "Email, parola, rolul și locația sunt obligatorii.", null);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (users.EmailExists(email))
        {
            return (false, "Există deja un cont cu acest email.", null);
        }

        var nameFromEmail = email.Split('@')[0].Trim();
        if (string.IsNullOrEmpty(nameFromEmail))
        {
            nameFromEmail = "User";
        }

        var displayName = string.IsNullOrWhiteSpace(request.Name)
            ? nameFromEmail
            : request.Name.Trim();

        var user = new User
        {
            Name = displayName,
            Email = email,
            PasswordHash = HashPassword(request.Password),
            Role = request.Role.Trim(),
            Location = request.Location.Trim()
        };

        users.Create(user);
        return (true, string.Empty, ToResponse(user));
    }

    public (bool Ok, string Error, AuthResponseDto? Data) Login(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Email și parola sunt obligatorii.", null);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = users.GetByEmail(email);
        if (user is null)
        {
            return (false, "Email sau parolă greșită.", null);
        }

        if (user.PasswordHash != HashPassword(request.Password))
        {
            return (false, "Email sau parolă greșită.", null);
        }

        return (true, string.Empty, ToResponse(user));
    }

    private static AuthResponseDto ToResponse(User user) =>
        new()
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };

    private static string HashPassword(string rawPassword)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawPassword);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}

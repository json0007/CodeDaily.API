using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.API.Domain.Models;
using CodeDaily.API.Features.Auth;
using CodeDaily.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace CodeDaily.API.Domain.Services;

public class AuthenticationResult
{
    public bool IsAuthenticated { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RegisterResult
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string UserId { get; set; } = default!;
}

public class AuthService(JwtConfiguration jwtConfig, IUserRepository userRepository)
{
    public async Task<RegisterResult> RegisterUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new RegisterResult
            {
                ErrorMessage = "Email and password are required."
            };
        }

        // Check if user already exists
        var existingUser = await userRepository.GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            return new RegisterResult
            {
                ErrorMessage = "User already exists."
            };
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.CreateUserAsync(user);

        return new RegisterResult
        {
            IsSuccessful = true,
            UserId = user.Id
        };
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthenticationResult
            {
                ErrorMessage = "Invalid username or password."
            };
        }

        var user = await userRepository.GetUserByEmailAsync(email);

        if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
        {
            return new AuthenticationResult
            {
                ErrorMessage = "Invalid username or password."
            };
        }

        var token = GenerateToken(user);
        await userRepository.UpdateUserAsync(user);

        return new AuthenticationResult
        {
            IsAuthenticated = true,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt
        };
    }

    private (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(jwtConfig.PrivateKey, SecurityAlgorithms.RsaSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtConfig.Expiration);

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials
        );

        user.LastLoginAt = DateTime.UtcNow;
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private bool VerifyPasswordHash(string password, string passwordHash)
    {
        // Using BCrypt for password verification
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    private string HashPassword(string password)
    {
        // Using BCrypt to hash password with work factor of 12
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}

public class JsonWebKey
{
    public string Kty { get; set; } = string.Empty;
    public string Use { get; set; } = string.Empty;
    public string Kid { get; set; } = string.Empty;
    public string Alg { get; set; } = string.Empty;
    public string N { get; set; } = string.Empty;
    public string E { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.API.Domain.Models;
using CodeDaily.Domain.Models;

namespace CodeDaily.API.Infrastructure.Db.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private static readonly Dictionary<string, User> _users = new()
    {
        ["a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"] = new User
        {
            Id = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
            Email = "admin@test.com",
            // Hash for "Password@1" using BCrypt work factor 12
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@1", workFactor: 12),
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string> { "Admin", "User" }
        }
    };

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(string id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task<bool> UpdateUserAsync(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            return Task.FromResult(false);
        }

        _users[user.Id] = user;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteUserAsync(string id)
    {
        return Task.FromResult(_users.Remove(id));
    }
}

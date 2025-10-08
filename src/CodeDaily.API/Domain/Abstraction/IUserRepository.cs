using CodeDaily.API.Domain.Models;
using CodeDaily.Domain.Models;

namespace CodeDaily.API.Domain.Abstraction;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(string id);
    Task<User> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);
}
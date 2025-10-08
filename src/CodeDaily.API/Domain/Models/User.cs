namespace CodeDaily.API.Domain.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = [];
    
}
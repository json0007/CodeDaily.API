using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.API.Domain.Models;
using CodeDaily.API.Models;

namespace CodeDaily.API.Infrastructure.Db.Repositories;

public class DynamoDbUserRepository(IDynamoDBContext dynamoDbContext) : IUserRepository
{
    /// <summary>
    /// Get a user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(string id)
    {
        var dynamoDbUser = await dynamoDbContext.LoadAsync<DynamoDbUser>(id);

        if (dynamoDbUser == null)
        {
            return null;
        }

        return MapToUser(dynamoDbUser);
    }

    /// <summary>
    /// Get a user by email using the EmailIndex GSI
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var config = new QueryOperationConfig
        {
            IndexName = "EmailIndex",
            Limit = 1,
            KeyExpression = new Expression
            {
                ExpressionStatement = "Email = :email",
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":email", email }
                }
            }
        };

        var search = dynamoDbContext.FromQueryAsync<DynamoDbUser>(config);
        var results = await search.GetRemainingAsync();

        if (results.Count == 0)
        {
            return null;
        }

        return MapToUser(results[0]);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task<User> CreateUserAsync(User user)
    {
        var existingUser = await GetUserByEmailAsync(user.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with the same email already exists.");
        }

        var dynamoDbUser = new DynamoDbUser
        {
            PK = Guid.NewGuid().ToString(),
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles
        };

        await dynamoDbContext.SaveAsync(dynamoDbUser);

        user.Id = dynamoDbUser.PK;
        return user;
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    public async Task<bool> UpdateUserAsync(User user)
    {
        var existingUser = await dynamoDbContext.LoadAsync<DynamoDbUser>(user.Id);

        if (existingUser == null)
        {
            return false;
        }

        var dynamoDbUser = new DynamoDbUser
        {
            PK = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles
        };

        await dynamoDbContext.SaveAsync(dynamoDbUser);

        return true;
    }

    /// <summary>
    /// Delete a user by ID
    /// </summary>
    public async Task<bool> DeleteUserAsync(string id)
    {
        var existingUser = await dynamoDbContext.LoadAsync<DynamoDbUser>(id);

        if (existingUser == null)
        {
            return false;
        }

        await dynamoDbContext.DeleteAsync<DynamoDbUser>(id);

        return true;
    }

    private static User MapToUser(DynamoDbUser dynamoDbUser)
    {
        return new User
        {
            Id = dynamoDbUser.PK,
            Email = dynamoDbUser.Email,
            PasswordHash = dynamoDbUser.PasswordHash,
            CreatedAt = dynamoDbUser.CreatedAt,
            LastLoginAt = dynamoDbUser.LastLoginAt,
            Roles = dynamoDbUser.Roles
        };
    }
}

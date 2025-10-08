using Amazon.DynamoDBv2.DataModel;

namespace CodeDaily.API.Models;

[DynamoDBTable("codedaily-users")]
public class DynamoDbUser
{
    [DynamoDBHashKey]
    public string PK { get; set; } = default!;

    [DynamoDBGlobalSecondaryIndexHashKey("EmailIndex")]
    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public List<string> Roles { get; set; } = [];
}
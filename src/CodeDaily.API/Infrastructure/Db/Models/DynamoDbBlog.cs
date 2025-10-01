using Amazon.DynamoDBv2.DataModel;
using CodeDaily.API.Constants;

namespace CodeDaily.API.Models;

[DynamoDBTable("codedaily-blogs")]
public class DynamoDbBlog
{
    [DynamoDBHashKey]
    public string PK { get; set; } = default!;
    [DynamoDBGlobalSecondaryIndexHashKey("StatusDateIndex")]
    public string Status { get; set; } = BlogStatus.Draft;
    [DynamoDBGlobalSecondaryIndexRangeKey("StatusDateIndex")]
    public string SortDate { get; set; } = default!;
    [DynamoDBGlobalSecondaryIndexHashKey("SlugIndex")]
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Author { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string[] Tags { get; set; } = [];
    public bool IsFeatured { get; set; }
    public int? ReadTime { get; set; }
}
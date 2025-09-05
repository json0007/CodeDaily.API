using Amazon.DynamoDBv2.DataModel;

namespace CodeDaily.API.Models;

[DynamoDBTable("codedaily-blogs-test")]
public class BlogPost
{
    [DynamoDBHashKey] // Status is the partition key
    public string Status { get; set; } = "draft"; // "published" or "draft"
    
    [DynamoDBRangeKey] // PublishedDate is the sort key
    public DateTime PublishedDate { get; set; }
    
    [DynamoDBLocalSecondaryIndexRangeKey("PublishedTitleIndex")]
    public string Title { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    public string Content { get; set; } = default!;

    [DynamoDBGlobalSecondaryIndexHashKey("SlugIndex")]
    public string Slug { get; set; } = default!;
    
    [DynamoDBLocalSecondaryIndexRangeKey("PublishedTagIndex")]
    public string TagString { get; set; } = default!;
    
    [DynamoDBLocalSecondaryIndexRangeKey("DraftAuthorIndex")]
    public string Author { get; set; } = default!;
    
    [DynamoDBIgnore]
    public string[] Tags => TagString?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    
    public bool IsFeatured { get; set; }
    public int? ReadTime { get; set; }
}
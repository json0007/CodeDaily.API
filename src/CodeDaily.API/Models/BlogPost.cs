using Amazon.DynamoDBv2.DataModel;

namespace CodeDaily.API.Models;

[DynamoDBTable("blog-posts")]
public class BlogPost
{
    [DynamoDBHashKey]
    public string Id { get; set; } = default!;
    
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public DateTime PublishedDate { get; set; }
    
    public string Content { get; set; } = default!;

     [DynamoDBGlobalSecondaryIndexHashKey("Tag-PublishedDate-index")]
    public List<string> Tags { get; set; } = new();
    
    [DynamoDBGlobalSecondaryIndexHashKey("Slug-index")]
    public string Slug { get; set; } = default!;
    
    public bool IsFeatured { get; set; }


    // Optional fields
    public int? ReadTime { get; set; } // in minutes
    public string? Author { get; set; } // for future use
}


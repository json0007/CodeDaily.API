using Amazon.DynamoDBv2.DataModel;

namespace CodeDaily.API.Models;

public class BlogListItem
{
    public string Id { get; set; } = default!;
    
    public string Title { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public DateTime PublishedDate { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public string Slug { get; set; } = default!;
    
    public bool IsFeatured { get; set; }

    public int? ReadTime { get; set; } // in minutes
}
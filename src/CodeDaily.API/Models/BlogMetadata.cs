namespace CodeDaily.API.Models;

public class BlogMetadata
{
    public string Id { get; set; } = default!;
    
    public string Title { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public DateTime PublishedDate { get; set; }

    public string TagString { get; set; } = default!;
    public string[] Tags => TagString?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
    
    public string Slug { get; set; } = default!;
    
    public bool IsFeatured { get; set; }

    public int? ReadTime { get; set; } // in minutes
}
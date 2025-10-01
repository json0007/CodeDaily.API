namespace CodeDaily.Domain.Models;

public class Blog : BlogMetadata
{
    public string Content { get; set; } = default!;
}

public class BlogMetadata
{
    public string Id { get; set; } = default!;

    public string Slug { get; set; } = default!;
    
    public string Title { get; set; } = default!;
    
    public string Description { get; set; } = default!;

    public string Author{ get; set; } = default!;

    public string Status { get; set; } = default!;
    
    public DateTime? PublishedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public string[] Tags { get; set; } = Array.Empty<string>();
    
    public bool IsFeatured { get; set; }

    public int? ReadTime { get; set; } // in minutes
}
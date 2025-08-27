namespace CodeDaily.API.Data;

public class BlogItem
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime PublishedDate { get; set; }
    public string Content { get; set; } = default!;
    public List<string> Tags { get; set; } = new();
    public string Slug { get; set; } = default!;
    public bool IsFeatured { get; set; }

    // Optional fields
    public int? ReadTime { get; set; } // in minutes
    public string? Author { get; set; } // for future use
}


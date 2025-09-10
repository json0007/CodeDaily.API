using CodeDaily.API.Constants;

namespace CodeDaily.API.Features.Blogs;

public class BlogPostCommand
{
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Status { get; set; } = BlogPostStatus.Draft;
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public int? ReadTime { get; set; }
}
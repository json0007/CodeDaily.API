namespace CodeDaily.API.Features.Blogs;

public class UpdateBlogCommand : CreateBlogCommand
{
    public string Id { get; set; } = default!;
}

public class CreateBlogCommand
{
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string[] Tags { get; set; } = [];
    public bool IsFeatured { get; set; }
    public int? ReadTime { get; set; }
}
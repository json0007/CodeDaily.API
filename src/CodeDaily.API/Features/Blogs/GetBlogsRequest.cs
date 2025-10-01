using System.ComponentModel.DataAnnotations;
using CodeDaily.API.Constants;

namespace CodeDaily.API.Features.Blogs;

public class GetBlogsQuery
{
    public string PageKey { get; set; } = default!;
    public string? Title { get; set; }
    public string[]? Tags { get; set; }
}
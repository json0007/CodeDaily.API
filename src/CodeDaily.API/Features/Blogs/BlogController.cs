using CodeDaily.API.Constants;
using CodeDaily.API.Domain.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Features.Blogs;

[Route("api/[controller]")]
public class BlogsController(IBlogRepository blogPostRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var blogListItems = await blogPostRepository.GetByStatusAsync(BlogStatus.Published);
        return Ok(blogListItems);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var blogPost = await blogPostRepository.GetBySlugAsync(slug);
        if (blogPost == null || blogPost.Status != BlogStatus.Published)
        {
            return NotFound();
        }
        return Ok(blogPost);
    }
}
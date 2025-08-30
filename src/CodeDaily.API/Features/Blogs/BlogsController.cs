using CodeDaily.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Features.Blogs;

[Route("api/[controller]")]
public class BlogsController(IBlogPostRepository blogPostRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var blogListItems = await blogPostRepository.GetAllAsync();
        return Ok(blogListItems);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Get(string slug)
    {
        var blogPost = await blogPostRepository.GetBySlugAsync(slug);
        if (blogPost == null)
        {
            return NotFound();
        }
        return Ok(blogPost);
    }
}
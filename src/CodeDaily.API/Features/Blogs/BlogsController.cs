using CodeDaily.API.Constants;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Features.Blogs;

[Route("api/[controller]")]
public class BlogsController(IBlogRepository blogPostRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var blogListItems = await blogPostRepository.GetAllByStatusAsync(BlogStatus.Published);
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

    // [HttpPost]
    // public async Task<IActionResult> Post([FromBody] CreateBlogCommand command)
    // {
    //     var blogPost = new Blog
    //     {
    //         Slug = command.Slug,
    //         Title = command.Title,
    //         Description = command.Description,
    //         Content = command.Content,
    //         Author = command.Author,
    //         Status = command.Status,
    //         CreatedDate = DateTime.UtcNow,
    //         PublishedDate = command.Status == BlogStatus.Published ? DateTime.UtcNow : null,
    //         Tags = command.Tags,
    //         IsFeatured = command.IsFeatured,
    //         ReadTime = command.ReadTime
    //     };

    //     await blogPostRepository.CreateAsync(blogPost);
    //     return NoContent();
    // }
}
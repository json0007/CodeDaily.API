using CodeDaily.API.Constants;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Features.Blogs;

[Authorize(AuthenticationSchemes = "Bearer")]
[Route("api/admin/blogs")]

public class BlogAdminController(IBlogRepository blogPostRepository) : ControllerBase

{
    [HttpGet("{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var blogListItems = await blogPostRepository.GetByStatusAsync(status);
        return Ok(blogListItems);
    }
    
    [HttpGet("/{status}/{id}")]
    public async Task<IActionResult> Get(string status, string id)
    {
        var blogPost = await blogPostRepository.GetByIdAsync(id);
        if (blogPost == null && blogPost?.Status != status)
        {
            return NotFound();
        }
    
        return Ok(blogPost);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateBlogCommand command)
    {
        var blogPost = new Blog
        {
            Slug = command.Slug,
            Title = command.Title,
            Description = command.Description,
            Content = command.Content,
            Author = command.Author,
            Status = command.Status,
            CreatedDate = DateTime.UtcNow,
            PublishedDate = command.Status == BlogStatus.Published ? DateTime.UtcNow : null,
            Tags = command.Tags,
            IsFeatured = command.IsFeatured,
            ReadTime = command.ReadTime
        };
    
        var id = await blogPostRepository.CreateAsync(blogPost);
        return Ok(id);
    }
    
    [HttpPut("id")]
    public async Task<IActionResult> Put(string id, [FromBody] CreateBlogCommand command)
    {
        if (await blogPostRepository.GetByIdAsync(id) == null)
        {
            return NotFound("Blog not found");
        }
        
        var blogPost = new Blog
        {
            Id = id,
            Slug = command.Slug,
            Title = command.Title,
            Description = command.Description,
            Content = command.Content,
            Author = command.Author,
            Status = command.Status,
            PublishedDate = command.Status == BlogStatus.Published ? DateTime.UtcNow : null,
            Tags = command.Tags,
            IsFeatured = command.IsFeatured,
            ReadTime = command.ReadTime
        };
    
        await blogPostRepository.UpdateAsync(blogPost);
        return NoContent();
    }
}
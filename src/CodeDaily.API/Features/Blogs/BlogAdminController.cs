using CodeDaily.API.Constants;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Features.Blogs;

[Authorize]
[Route("api/admin/[controller]")]
#pragma warning disable CS9113 // Parameter is unread.

public class BlogAdminController(IBlogRepository blogPostRepository) : ControllerBase
#pragma warning restore CS9113 // Parameter is unread.

{
    // [HttpGet("by-status/{status}")]
    // public async Task<IActionResult> GetByStatus(string status)
    // {
    //     var blogListItems = await blogPostRepository.GetByStatusAsync(status);
    //     return Ok(blogListItems);
    // }
    //
    // [HttpGet("{id}")]
    // public async Task<IActionResult> Get(string id)
    // {
    //     var blogPost = await blogPostRepository.GetByIdAsync(id);
    //     if (blogPost == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     return Ok(blogPost);
    // }
    //
    // [HttpPost]
    // public async Task<IActionResult> Post([FromBody] BlogCommand command)
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
    //
    //     await blogPostRepository.CreateAsync(blogPost);
    //     return NoContent();
    // }
    //
    // [HttpPut("id)]
    // public async Task<IActionResult> Put(string, id, [FromBody] BlogCommand command)
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
    //
    //     await blogPostRepository.CreateAsync(blogPost);
    //     return NoContent();
    // }
}
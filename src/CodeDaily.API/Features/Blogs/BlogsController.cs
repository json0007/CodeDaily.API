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
    // public async Task<IActionResult> Post([FromBody] BlogPostCommand command)
    // {
    //     var blogPost = new BlogPost
    //     {
    //         Slug = command.Slug,
    //         Title = command.Title,
    //         Description = command.Description,
    //         Content = command.Content,
    //         Author = command.Author,
    //         Status = command.Status,
    //         CreatedDate = command.CreatedDate,
    //         PublishedDate = command.PublishedDate,
    //         Tags = command.Tags,
    //         IsFeatured = command.IsFeatured,
    //         ReadTime = command.ReadTime
    //     };

    //     var createdBlogPost = await blogPostRepository.CreateAsync(blogPost);
    //     return CreatedAtAction(nameof(Get), new { slug = createdBlogPost.Slug }, createdBlogPost);
    // }
}
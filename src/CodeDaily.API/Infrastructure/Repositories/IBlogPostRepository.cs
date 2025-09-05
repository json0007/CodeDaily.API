using CodeDaily.API.Models;

namespace CodeDaily.API.Repositories;

public interface IBlogPostRepository
{
    Task<IEnumerable<BlogListItem>> GetAllPublishedAsync(); 
    Task<BlogPost?> GetByIdAsync(string id);
    Task<BlogPost?> GetBySlugAsync(string slug);
    Task<BlogPost> CreateAsync(BlogPost blogPost);
    Task<BlogPost> UpdateAsync(BlogPost blogPost);
    Task DeleteAsync(string id);
}
using CodeDaily.API.Models;

namespace CodeDaily.API.Repositories;

public interface IBlogPostRepository
{
    Task<IEnumerable<BlogMetadata>> GetAllPublishedAsync(); 
    Task<Blog?> GetBySlugAsync(string slug);
    Task<BlogPost> CreateAsync(BlogPost blogPost);
    Task<BlogPost> UpdateAsync(BlogPost blogPost);
    Task DeleteAsync(string slug);
}
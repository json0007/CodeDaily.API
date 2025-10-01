using CodeDaily.API.Models;
using CodeDaily.Domain.Models;

namespace CodeDaily.API.Domain.Abstraction;

public interface IBlogRepository
{
    Task<Blog?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<BlogMetadata>> GetAllByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Blog?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task CreateAsync(Blog blogPost, CancellationToken cancellationToken = default);
    Task UpdateAsync(Blog blogPost, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
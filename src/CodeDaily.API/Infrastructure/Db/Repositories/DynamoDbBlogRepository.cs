using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CodeDaily.API.Constants;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.API.Models;
using CodeDaily.Domain.Models;

namespace CodeDaily.API.Infrastructure.Db.Repositories;


public class DynamoDbBlogRepository : IBlogRepository
{
    private readonly IDynamoDBContext _dynamoDbContext;

    public DynamoDbBlogRepository(IDynamoDBContext dynamoDbContext)
    {
        _dynamoDbContext = dynamoDbContext;
    }

    /// <summary>
    /// Get a blog post by ID
    /// </summary>
    public async Task<Blog?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var dynamoDbBlog = await _dynamoDbContext.LoadAsync<DynamoDbBlog>(id, cancellationToken);
        
        if (dynamoDbBlog == null)
        {
            return null;
        }

        return MapToBlog(dynamoDbBlog);
    }

    /// <summary>
    /// Get a blog post by slug
    /// </summary>
    public async Task<Blog?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var config = new QueryOperationConfig
        {
            IndexName = "SlugIndex",
            Limit = 1,
            KeyExpression = new Expression
            {
                ExpressionStatement = "Slug = :slug",
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":slug", slug }
                }
            }
        };
        
        var search = _dynamoDbContext.FromQueryAsync<DynamoDbBlog>(config);
        var results = await search.GetRemainingAsync(cancellationToken);
        
        if (results.Count == 0)
        {
            return null;
        }

        return MapToBlog(results[0]);
    }

    /// <summary>
    /// Get all blog posts by status, sorted by date (newest first)
    /// </summary>
    public async Task<List<BlogMetadata>> GetAllByStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        var config = new QueryOperationConfig
        {
            IndexName = "StatusDateIndex",
            Select = SelectValues.SpecificAttributes,
            AttributesToGet = new List<string>
            {
                "PK", "Status", "Slug", "Title", "Description", "Tags", "Author",
                "CreatedDate", "PublishedDate", "SortDate", "IsFeatured", "ReadTime"
            },
            KeyExpression = new Expression
            {
                ExpressionStatement = "#status = :status",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#status", "Status" }
                },
                ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                {
                    { ":status", status }
                }
            },
            BackwardSearch = true // Newest first
        };
        
        var search = _dynamoDbContext.FromQueryAsync<DynamoDbBlog>(config);
        var results = await search.GetRemainingAsync(cancellationToken);
        
        return results.Select(MapToBlogMetadata).ToList();
    }

    /// <summary>
    /// Create a new blog post
    /// </summary>
    public async Task CreateAsync(Blog blogPost, CancellationToken cancellationToken = default)
    {
        var existingBlog = await GetBySlugAsync(blogPost.Slug, cancellationToken);

        if (existingBlog != null)
        {
            throw new InvalidOperationException("Blog post with the same slug already exists.");
        }

        // If publishing, ensure PublishedDate is set
        if (blogPost.Status == BlogStatus.Published && !blogPost.PublishedDate.HasValue)
        {
            blogPost.PublishedDate = DateTime.UtcNow;
        }
        else if (blogPost.Status != BlogStatus.Published)
        {
            blogPost.PublishedDate = null; // Clear PublishedDate if not published
        }

        // Determine SortDate based on status and published date    
        var sortDate = blogPost.Status == BlogStatus.Published ?
            blogPost.PublishedDate!.Value.ToString("o") :
            blogPost.CreatedDate.ToString("o");

        var dynamoDbBlog = new DynamoDbBlog
        {
            PK = Guid.NewGuid().ToString(),
            Status = blogPost.Status,
            Slug = blogPost.Slug,
            Title = blogPost.Title,
            Description = blogPost.Description,
            Content = blogPost.Content,
            Author = blogPost.Author,
            CreatedDate = blogPost.CreatedDate,
            PublishedDate = blogPost.PublishedDate,
            SortDate = sortDate,
            Tags = blogPost.Tags,
            IsFeatured = blogPost.IsFeatured,
            ReadTime = blogPost.ReadTime
        };

        await _dynamoDbContext.SaveAsync(dynamoDbBlog, cancellationToken);
    }

    /// <summary>
    /// Update a blog post
    /// </summary>
    public async Task UpdateAsync(Blog blogPost, CancellationToken cancellationToken = default)
    {
             // If publishing, ensure PublishedDate is set
        if (blogPost.Status == BlogStatus.Published && !blogPost.PublishedDate.HasValue)
        {
            blogPost.PublishedDate = DateTime.UtcNow;
        }
        else if (blogPost.Status != BlogStatus.Published)
        {
            blogPost.PublishedDate = null; // Clear PublishedDate if not published
        }

        // Determine SortDate based on status and published date    
        var sortDate = blogPost.Status == BlogStatus.Published ?
            blogPost.PublishedDate!.Value.ToString("o") :
            blogPost.CreatedDate.ToString("o");

        var dynamoDbBlog = new DynamoDbBlog
        {
            PK = blogPost.Id,
            Status = blogPost.Status,
            Slug = blogPost.Slug,
            Title = blogPost.Title,
            Description = blogPost.Description,
            Content = blogPost.Content,
            Author = blogPost.Author,
            CreatedDate = blogPost.CreatedDate,
            PublishedDate = blogPost.PublishedDate,
            SortDate = sortDate,
            Tags = blogPost.Tags,
            IsFeatured = blogPost.IsFeatured,
            ReadTime = blogPost.ReadTime
        };

        await _dynamoDbContext.SaveAsync(dynamoDbBlog, cancellationToken);
    }

    /// <summary>
    /// Delete a blog post by ID
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _dynamoDbContext.DeleteAsync<DynamoDbBlog>(id, cancellationToken);
    }

    private static Blog MapToBlog(DynamoDbBlog dynamoDbBlog)
    {
        return new Blog
        {
            Id = dynamoDbBlog.PK,
            Status = dynamoDbBlog.Status,
            Slug = dynamoDbBlog.Slug,
            Title = dynamoDbBlog.Title,
            Description = dynamoDbBlog.Description,
            Content = dynamoDbBlog.Content,
            Tags = dynamoDbBlog.Tags,
            Author = dynamoDbBlog.Author,
            CreatedDate = dynamoDbBlog.CreatedDate,
            PublishedDate = dynamoDbBlog.PublishedDate,
            IsFeatured = dynamoDbBlog.IsFeatured,
            ReadTime = dynamoDbBlog.ReadTime
        };
    }

    private static BlogMetadata MapToBlogMetadata(DynamoDbBlog dynamoDbBlog)
    {
        return new BlogMetadata
        {
            Id = dynamoDbBlog.PK,
            Status = dynamoDbBlog.Status,
            Slug = dynamoDbBlog.Slug,
            Title = dynamoDbBlog.Title,
            Description = dynamoDbBlog.Description,
            Tags = dynamoDbBlog.Tags,
            Author = dynamoDbBlog.Author,
            CreatedDate = dynamoDbBlog.CreatedDate,
            PublishedDate = dynamoDbBlog.PublishedDate,
            IsFeatured = dynamoDbBlog.IsFeatured,
            ReadTime = dynamoDbBlog.ReadTime
        };
    }
}


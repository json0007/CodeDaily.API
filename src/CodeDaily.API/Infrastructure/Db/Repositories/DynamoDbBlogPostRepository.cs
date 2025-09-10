using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CodeDaily.API.Constants;
using CodeDaily.API.Models;
using CodeDaily.API.Repositories;

namespace CodeDaily.API.Infrastructure.Repositories;

public class DynamoDbBlogPostRepository(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient)
    : IBlogPostRepository
{
    private readonly string _tableName = "codedaily-blogs";

    public async Task<IEnumerable<BlogMetadata>> GetAllPublishedAsync()
    {
        var table = new TableBuilder(dynamoDbClient, _tableName)
            .AddHashKey("GSI1_PK", DynamoDBEntryType.String)
            .AddRangeKey("GSI1_SK", DynamoDBEntryType.String)
            .Build();
        
        // Query GSI1 (StatusDateIndex) for published posts, sorted by date (descending)
        var queryConfig = new QueryOperationConfig
        {
            IndexName = "StatusDateIndex",
            KeyExpression = new Expression
            {
                ExpressionStatement = "GSI1_PK = :status",
                ExpressionAttributeValues = { { ":status", $"STATUS#{BlogPostStatus.Published}" } }
            },
            BackwardSearch = true, // Sort descending by date
            AttributesToGet = new List<string> { "Slug", "Title", "Description", "PublishedDate", "Tags", "IsFeatured", "ReadTime" },
            Select = SelectValues.SpecificAttributes
        };
        
        var search = table.Query(queryConfig);
        var documents = await search.GetRemainingAsync();
        
        return documents.Select(doc => new BlogMetadata
        {
            Id = doc["Slug"],
            Title = doc["Title"],
            Description = doc.ContainsKey("Description") ? doc["Description"] : "",
            PublishedDate = doc["PublishedDate"].AsDateTime(),
            TagString = string.Join(",", doc.ContainsKey("Tags") ? doc["Tags"].AsListOfString() : new List<string>()),
            Slug = doc["Slug"],
            IsFeatured = doc.ContainsKey("IsFeatured") && doc["IsFeatured"].AsBoolean(),
            ReadTime = doc.ContainsKey("ReadTime") ? doc["ReadTime"].AsInt() : null
        }).ToList();
    }

    private async Task<BlogPost?> GetBlogPostBySlugAsync(string slug)
    {
        var pk = $"BLOG#{slug}";
        
        var table = new TableBuilder(dynamoDbClient, _tableName)
            .AddHashKey("PK", DynamoDBEntryType.String)
            .AddRangeKey("SK", DynamoDBEntryType.String)
            .Build();
        var queryConfig = new QueryOperationConfig
        {
            KeyExpression = new Expression
            {
                ExpressionStatement = "PK = :pk",
                ExpressionAttributeValues = { { ":pk", pk } }
            }
        };
        
        var search = table.Query(queryConfig);
        var documents = await search.GetRemainingAsync();
        var document = documents.FirstOrDefault();
        
        if (document == null) return null;

        return dynamoDbContext.FromDocument<BlogPost>(document);
    }

    public async Task<Blog?> GetBySlugAsync(string slug)
    {
        var blogPost = await GetBlogPostBySlugAsync(slug);
        if (blogPost == null) return null;

        // Map from BlogPost to Blog model, ignoring PK/SK/GSI fields
        return new Blog
        {
            Id = blogPost.Slug,
            Title = blogPost.Title,
            Description = blogPost.Description,
            Content = blogPost.Content,
            PublishedDate = blogPost.PublishedDate ?? blogPost.CreatedDate,
            TagString = string.Join(",", blogPost.Tags),
            Slug = blogPost.Slug,
            IsFeatured = blogPost.IsFeatured,
            ReadTime = blogPost.ReadTime
        };
    }


    public async Task<BlogPost> CreateAsync(BlogPost blogPost)
    {
        // Setup the keys before saving
        blogPost.SetupKeys();
        
        // Save blog post
        await dynamoDbContext.SaveAsync(blogPost);
        
        // Create tag entries if there are tags
        if (blogPost.Tags?.Any() == true)
        {
            var tagTasks = blogPost.Tags.Select(tag => 
                dynamoDbContext.SaveAsync(BlogTag.FromBlogPost(blogPost, tag))
            );
            await Task.WhenAll(tagTasks);
        }
        
        return blogPost;
    }

    public async Task<BlogPost> UpdateAsync(BlogPost blogPost)
    {
        // Setup the keys before saving
        blogPost.SetupKeys();
        await dynamoDbContext.SaveAsync(blogPost);
        
        // Handle tag updates - delete old tags and create new ones
        // First, delete existing tags for this blog
        var table = new TableBuilder(dynamoDbClient, _tableName).Build();
        var queryConfig = new QueryOperationConfig
        {
            KeyExpression = new Expression
            {
                ExpressionStatement = "PK = :pk AND begins_with(SK, :tagPrefix)",
                ExpressionAttributeValues = { { ":pk", blogPost.PK }, { ":tagPrefix", "TAG#" } }
            }
        };
        
        var search = table.Query(queryConfig);
        var existingTagDocs = await search.GetRemainingAsync();
        
        // Delete existing tag entries
        var deleteTasks = existingTagDocs.Select(doc => 
            dynamoDbContext.DeleteAsync<BlogTag>(doc["PK"], doc["SK"])
        );
        await Task.WhenAll(deleteTasks);
        
        // Create new tag entries
        if (blogPost.Tags?.Any() == true)
        {
            var tagTasks = blogPost.Tags.Select(tag => 
                dynamoDbContext.SaveAsync(BlogTag.FromBlogPost(blogPost, tag))
            );
            await Task.WhenAll(tagTasks);
        }
        
        return blogPost;
    }

    public async Task DeleteAsync(string slug)
    {
        // Soft delete - get the BlogPost and update status
        var blogPost = await GetBlogPostBySlugAsync(slug);
        if (blogPost != null)
        {
            blogPost.Status = BlogPostStatus.Deleted;
            await UpdateAsync(blogPost);
        }
    }
}
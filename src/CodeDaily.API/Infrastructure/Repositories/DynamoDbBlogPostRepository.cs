using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CodeDaily.API.Models;
using CodeDaily.API.Repositories;

namespace CodeDaily.API.Infrastructure.Repositories;

public class DynamoDbBlogPostRepository(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient)
    : IBlogPostRepository
{
    private readonly string _tableName = Environment.GetEnvironmentVariable("BLOGS_TABLE_NAME") ?? "codedaily-blogs-test";

    public async Task<IEnumerable<BlogListItem>> GetAllPublishedAsync()
    {
        var table = Table.LoadTable(dynamoDbClient, _tableName);
        
        // Query for published posts, sorted by PublishedDate (descending)
        var queryConfig = new QueryOperationConfig
        {
            KeyExpression = new Expression
            {
                ExpressionStatement = "#status = :status",
                ExpressionAttributeNames = { { "#status", "Status" } },
                ExpressionAttributeValues = { { ":status", "published" } }
            },
            BackwardSearch = true // Sort descending by PublishedDate
        };
        
        var search = table.Query(queryConfig);
        var documents = await search.GetRemainingAsync();
        
        return documents.Select(doc => new BlogListItem
        {
            Id = doc["Slug"],
            Title = doc["Title"],
            Description = doc.ContainsKey("Description") ? doc["Description"] : "",
            PublishedDate = doc["PublishedDate"].AsDateTime(),
            TagString = doc.ContainsKey("Tags") ? doc["TagsString"] : "",
            Slug = doc["Slug"],
            IsFeatured = doc.ContainsKey("IsFeatured") && doc["IsFeatured"].AsBoolean(),
            ReadTime = doc.ContainsKey("ReadTime") ? doc["ReadTime"].AsInt() : null
        }).ToList();
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        var table = Table.LoadTable(dynamoDbClient, _tableName);
        
        // Use SlugIndex GSI to find by slug
        var queryConfig = new QueryOperationConfig
        {
            IndexName = "SlugIndex",
            KeyExpression = new Expression
            {
                ExpressionStatement = "Slug = :slug",
                ExpressionAttributeValues = { { ":slug", slug } }
            }
        };
        
        var search = table.Query(queryConfig);
        var documents = await search.GetRemainingAsync();
        var document = documents.FirstOrDefault();
        
        if (document == null) return null;

        return new BlogPost
        {
           
            Title = document["Title"],
            Content = document.ContainsKey("Content") ? document["Content"] : "",
            Description = document.ContainsKey("Description") ? document["Description"] : "",
            PublishedDate = document["PublishedDate"].AsDateTime(),
            TagString = document.ContainsKey("Tags") ? document["TagString"] : "",
            Slug = document["Slug"],
            IsFeatured = document.ContainsKey("IsFeatured") && document["IsFeatured"].AsBoolean(),
            ReadTime = document.ContainsKey("ReadTime") ? document["ReadTime"].AsInt() : null
        };
    }

    // Keep your existing create/update/delete methods unchanged for now
    public async Task<BlogPost?> GetByIdAsync(string id)
    {
        return await dynamoDbContext.LoadAsync<BlogPost>(id);
    }

    public async Task<BlogPost> CreateAsync(BlogPost blogPost)
    {      
        await dynamoDbContext.SaveAsync(blogPost);
        return blogPost;
    }

    public async Task<BlogPost> UpdateAsync(BlogPost blogPost)
    {
        await dynamoDbContext.SaveAsync(blogPost);
        return blogPost;
    }

    public async Task DeleteAsync(string id)
    {
        await dynamoDbContext.DeleteAsync<BlogPost>(id);
    }
}
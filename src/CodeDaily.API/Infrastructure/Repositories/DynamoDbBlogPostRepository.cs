using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using CodeDaily.API.Models;
using CodeDaily.API.Repositories;

namespace CodeDaily.API.Infrastructure.Repositories;

public class DynamoDbBlogPostRepository(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDbClient)
    : IBlogPostRepository
{
    public async Task<IEnumerable<BlogListItem>> GetAllAsync()
    {
        var table = Table.LoadTable(dynamoDbClient, "blog-posts");
        var scanConfig = new ScanOperationConfig
        {
            Select = SelectValues.SpecificAttributes,
            AttributesToGet = new List<string> 
            { 
                "Id", "Title", "Description", "PublishedDate", 
                "Tags", "Slug", "IsFeatured", "ReadTime" 
            },
            
        };
        
        var search = table.Scan(scanConfig);
        var documents = await search.GetRemainingAsync();
        
        return documents.Select(doc => new BlogListItem
        {
            Id = doc["Id"],
            Title = doc["Title"],
            Description = doc["Description"],
            PublishedDate = doc["PublishedDate"].AsDateTime(),
            Tags = doc.ContainsKey("Tags") ? doc["Tags"].AsListOfString() : new List<string>(),
            Slug = doc["Slug"],
            IsFeatured = doc.ContainsKey("IsFeatured") && doc["IsFeatured"].AsBoolean(),
            ReadTime = doc.ContainsKey("ReadTime") ? doc["ReadTime"].AsInt() : null
        }).OrderByDescending(doc => doc.PublishedDate).ToList();
    }

    public async Task<BlogPost?> GetByIdAsync(string id)
    {
        return await dynamoDbContext.LoadAsync<BlogPost>(id);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug)
    {
        var scanConditions = new List<ScanCondition>
        {
            new ScanCondition("Slug", ScanOperator.Equal, slug)
        };
        var search = dynamoDbContext.ScanAsync<BlogPost>(scanConditions);
        var results = await search.GetRemainingAsync();
        return results.FirstOrDefault();
    }

    public async Task<BlogPost> CreateAsync(BlogPost blogPost)
    {
        if (string.IsNullOrEmpty(blogPost.Id))
        {
            blogPost.Id = Guid.NewGuid().ToString();
        }
        
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
using Amazon.DynamoDBv2.DataModel;

namespace CodeDaily.API.Models;

[DynamoDBTable("codedaily-blogs")]
public class BlogPost
{
    [DynamoDBHashKey] // PK = "BLOG#{Slug}"
    public string PK { get; set; } = default!;
    
    [DynamoDBRangeKey] // SK = "BLOG#{PublishedDate}" or "BLOG#{CreatedDate}"
    public string SK { get; set; } = default!;
    
    // Blog properties
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Status { get; set; } = "draft"; // "published" or "draft"
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public int? ReadTime { get; set; }
    
      // GSI1 - Status queries (published/draft blogs by date)
    [DynamoDBGlobalSecondaryIndexHashKey("StatusDateIndex")]
    public string GSI1_PK { get; set; } = default!; // "STATUS#published" or "STATUS#draft"
    
    [DynamoDBGlobalSecondaryIndexRangeKey("StatusDateIndex")]
    public string GSI1_SK { get; set; } = default!; // PublishedDate or CreatedDate
    
    // GSI2 - Author queries (all blogs by author, sorted by date)
    [DynamoDBGlobalSecondaryIndexHashKey("AuthorDateIndex")]
    public string GSI2_PK { get; set; } = default!; // "AUTHOR#{author-name}"
    
    [DynamoDBGlobalSecondaryIndexRangeKey("AuthorDateIndex")]
    public string GSI2_SK { get; set; } = default!; // PublishedDate or CreatedDate

        [DynamoDBGlobalSecondaryIndexHashKey("TitleDateIndex")]
    public string GSI3_PK { get; set; } = default!; // "AUTHOR#{author-name}"
    
    [DynamoDBGlobalSecondaryIndexRangeKey("TitleDateIndex")]
    public string GSI3_SK { get; set; } = default!; // PublishedDate or CreatedDate

    public void SetupKeys()
    {
        PK = $"BLOG#{Slug}";

        // Use PublishedDate if published, otherwise CreatedDate
        var sortDate = Status == "published" && PublishedDate.HasValue
            ? PublishedDate.Value
            : CreatedDate;

        SK = $"BLOG#{sortDate:yyyy-MM-ddTHH:mm:ssZ}";

        GSI1_PK = $"STATUS#{Status}";
        GSI1_SK = sortDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

        GSI2_PK = $"AUTHOR#{Author.ToLower()}";
        GSI2_SK = sortDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        
        GSI3_PK = $"TITLE#{Title.ToLower()}";
        GSI3_SK = sortDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}

[DynamoDBTable("codedaily-blogs")]
public class BlogTag()
{
    [DynamoDBHashKey] // PK = "BLOG#{Slug}" (same as the blog)
    public string PK { get; set; } = default!;
    
    [DynamoDBRangeKey] // SK = "TAG#{tagname}"
    public string SK { get; set; } = default!;
    
    // GSI3 - Tag queries (all blogs with specific tag, sorted by date)
    [DynamoDBGlobalSecondaryIndexHashKey("TagDateIndex")]
    public string GSI4_PK { get; set; } = default!; // "TAG#{tagname}"
    
    [DynamoDBGlobalSecondaryIndexRangeKey("TagDateIndex")]
    public string GSI4_SK { get; set; } = default!; // "{date}#{slug}"
    
    public static BlogTag FromBlogPost(BlogPost blog, string tagName)
    {
        var sortDate = blog.Status == "published" && blog.PublishedDate.HasValue 
            ? blog.PublishedDate.Value 
            : blog.CreatedDate;
            
        var tag = new BlogTag
        {
            PK = $"BLOG#{blog.Slug}",      
            SK = $"TAG#{tagName}",       
            GSI4_PK = $"TAG#{tagName}", 
            GSI4_SK = $"BLOG#{sortDate:yyyy-MM-ddTHH:mm:ssZ}"
        };
        
        return tag;
    }
}
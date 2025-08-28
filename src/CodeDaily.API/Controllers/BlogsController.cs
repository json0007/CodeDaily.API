using CodeDaily.API.Data;
using CodeDaily.API.Model;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Controllers;

[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(items);
    }

    [HttpGet("{slug}")]
    public IActionResult Get(string slug)
    {
        var item = items.FirstOrDefault(i => i.Slug == slug);
        if (item == null)
        {
            return NotFound();
        }
        return Ok(item);
    }

    private static readonly List<BlogListItem> items = new List<BlogListItem>
    {
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Optimizing Azure Static Web Apps",
            Description = "A deep dive into flattening Angular builds for static hosting.",
            PublishedDate = DateTime.UtcNow.AddDays(-10),
            Tags = new List<string> { "Azure", "Angular", "CI/CD" },
            Slug = "optimizing-azure-static-web-apps",
            IsFeatured = true,
            ReadTime = 8
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Reverse Proxy Setup with Nginx on EC2",
            Description = "Step-by-step guide to deploying Dockerized apps behind Nginx.",
            PublishedDate = DateTime.UtcNow.AddDays(-20),
            Tags = new List<string> { "AWS", "Docker", "Nginx" },
            Slug = "reverse-proxy-nginx-ec2",
            IsFeatured = false,
            ReadTime = 6
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Flattening Angular Output for Static Hosting",
            Description = "Techniques to restructure build artifacts for clean deployment.",
            PublishedDate = DateTime.UtcNow.AddDays(-30),
            Tags = new List<string> { "Angular", "Static Hosting", "Build" },
            Slug = "flatten-angular-output",
            IsFeatured = false,
            ReadTime = 5
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Git Rebase vs Squash: Clean Commit Strategy",
            Description = "When and why to squash or rebase for maintainable history.",
            PublishedDate = DateTime.UtcNow.AddDays(-15),
            Tags = new List<string> { "Git", "Version Control", "Best Practices" },
            Slug = "git-rebase-vs-squash",
            IsFeatured = true,
            ReadTime = 4
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Secure SSH Key Management in Cloud Deployments",
            Description = "Avoiding common pitfalls in cloud access control.",
            PublishedDate = DateTime.UtcNow.AddDays(-25),
            Tags = new List<string> { "Security", "SSH", "Cloud" },
            Slug = "secure-ssh-key-management",
            IsFeatured = false,
            ReadTime = 7
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Cosmos DB vs MongoDB: Partitioning Deep Dive",
            Description = "Comparing partition strategies across two NoSQL giants.",
            PublishedDate = DateTime.UtcNow.AddDays(-35),
            Tags = new List<string> { "Cosmos DB", "MongoDB", "Partitioning" },
            Slug = "cosmos-vs-mongo-partitioning",
            IsFeatured = true,
            ReadTime = 9
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "CI/CD Pipeline Troubleshooting in GitHub Actions",
            Description = "Diagnosing subtle failures in multi-step workflows.",
            PublishedDate = DateTime.UtcNow.AddDays(-12),
            Tags = new List<string> { "CI/CD", "GitHub Actions", "DevOps" },
            Slug = "cicd-troubleshooting-github-actions",
            IsFeatured = false,
            ReadTime = 6
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Feature-Based Angular Architecture",
            Description = "Organizing scalable frontends with clean separation.",
            PublishedDate = DateTime.UtcNow.AddDays(-18),
            Tags = new List<string> { "Angular", "Architecture", "Frontend" },
            Slug = "feature-based-angular-architecture",
            IsFeatured = true,
            ReadTime = 7
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Evaluating Cloud OS Choices for Dev Environments",
            Description = "Balancing cost, control, and performance in cloud VMs.",
            PublishedDate = DateTime.UtcNow.AddDays(-22),
            Tags = new List<string> { "Cloud", "DevOps", "VMs" },
            Slug = "cloud-os-choices-dev-env",
            IsFeatured = false,
            ReadTime = 5
        },
        new BlogListItem {
            Id = Guid.NewGuid().ToString(),
            Title = "Security Hygiene in Enterprise Codebases",
            Description = "Practical steps to reduce risk in large-scale systems.",
            PublishedDate = DateTime.UtcNow.AddDays(-5),
            Tags = new List<string> { "Security", "Enterprise", "Code Quality" },
            Slug = "security-hygiene-enterprise-code",
            IsFeatured = true,
            ReadTime = 10
        }
    };
}
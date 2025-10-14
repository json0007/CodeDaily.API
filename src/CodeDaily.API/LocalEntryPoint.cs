using CodeDaily.API.Features.Auth;
using Microsoft.OpenApi.Models;

namespace CodeDaily.API;

/// <summary>
/// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
/// </summary>
public class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {

                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "CodeDaily API",
                        Version = "v1"
                    });

                    // Add JWT Authentication
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Get your token from POST /api/auth/login, then enter your token below.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());

                // Configure JWT with in-memory generated keys for local development
                var rsa = System.Security.Cryptography.RSA.Create(2048);
                var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
                var publicKeyPem = rsa.ExportRSAPublicKeyPem();
    
                var jwtConfig = new JwtConfiguration(privateKeyPem, publicKeyPem, 3600);

                services.AddSingleton(jwtConfig);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
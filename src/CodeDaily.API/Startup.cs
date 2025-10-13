using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CodeDaily.API.Domain.Abstraction;
using CodeDaily.API.Domain.Services;
using CodeDaily.API.Features.Auth;
using CodeDaily.API.Infrastructure.Db.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


namespace CodeDaily.API;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Configure AWS services
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddTransient<IDynamoDBContext, DynamoDBContext>();

        // Configure CORS for public readonly API
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        ConfigureAuthentication(services);
        services.AddAuthorization();
        
        services.AddScoped<IBlogRepository, DynamoDbBlogRepository>();
        services.AddScoped<AuthService>();
    }

    private void ConfigureAuthentication(IServiceCollection services)
    {
        // JWT configuration is now set up in LocalEntryPoint (from appsettings.json)
        // or LambdaEntryPoint (from SSM Parameter Store)
        var serviceProvider = services.BuildServiceProvider();
        var jwtConfig = serviceProvider.GetRequiredService<JwtConfiguration>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = jwtConfig.PublicKey,
                    ClockSkew = TimeSpan.Zero
                };
            
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 404;
                        return Task.CompletedTask;
                    },
                
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 404;
                        return Task.CompletedTask;
                    }
                };
            });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        
        app.UseCors();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}